using System.Collections;
using System.Collections.Generic;
using System.Text;
using GamificationPlayer.DTO.AnnounceDeviceFlow;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoAnnounceDeviceFlow(AnnounceDeviceFlowCallback onReady)
        {
            var moduleSession = new AnnounceDeviceFlowRequestDTO();

            string data = moduleSession.ToJson();

            sessionData.AddToLog(moduleSession.data);

            string webRequestString = string.Format("{0}/device-login", environmentConfig.API_URL);

            if(environmentConfig.TurnOnLogging) Debug.Log(data);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            using (UnityWebRequest webRequest = new UnityWebRequest(webRequestString, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                var loginUrl = string.Empty;

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
                        break;
                    case UnityWebRequest.Result.Success:
                        if(environmentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);

                        var response = webRequest.downloadHandler.text.FromJson<AnnounceDeviceFlowResponseDTO>();
                        sessionData.AddToLog(response.data, false);
                        loginUrl = response.data.attributes.url;
                        break;
                }

                onReady?.Invoke(webRequest.result, loginUrl);
            }
        }
    }
}
