using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.GetDeviceFlow;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetDeviceFlow(GetDeviceFlowCallback onReady)
        {
            if(!sessionData.TryGetLatestDeviceFlowId(out var deviceFlowId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError, false, string.Empty);

                yield break;
            }

            yield return CoGetDeviceFlow(deviceFlowId, onReady);
        }

        private IEnumerator CoGetDeviceFlow(Guid deviceFlowId, GetDeviceFlowCallback onReady)
        {
            string webRequestString = string.Format("{0}/device-login/{1}", enviromentConfig.API_URL, deviceFlowId);

            if(enviromentConfig.TurnOnLogging) Debug.Log(webRequestString);

            UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
            webRequest.SetRequestHeader("X-Api-Key", enviromentConfig.APIKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            yield return webRequest.SendWebRequest();

            var isValidated = false;
            var userId = string.Empty;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("Connection Error.");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    if(enviromentConfig.TurnOnLogging) Debug.LogError(":\nReceived: " + webRequest.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.Success:
                    if(enviromentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    var response = webRequest.downloadHandler.text.FromJson<GetDeviceFlowResponseDTO>();
                    
                    response.data.attributes.is_validated = enviromentConfig.IsMockServer || response.data.attributes.is_validated;
                    
                    sessionData.AddToLog(response.data);
                    isValidated = response.data.attributes.is_validated;
                    userId = response.data.attributes.user_id;
                    break;
            }

            onReady?.Invoke(webRequest.result, isValidated, userId);
        }
    }
}
