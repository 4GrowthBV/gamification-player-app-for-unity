using System;
using System.Collections;
using GamificationPlayer.Session;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoStoreTakeAwayResult(byte[] fileData,
            StoreTakeAwayResultCallback onReady = null, 
            Guid takeAwaySessionId = default)
        {
            if(takeAwaySessionId == default)
            {
                sessionData.TryGetLatestId<TakeAwaySessionId>(out takeAwaySessionId);
            }

            yield return CoStoreTakeAwayResult(fileData, takeAwaySessionId, onReady);
        }

        private IEnumerator CoStoreTakeAwayResult(byte[] fileData, 
            Guid takeAwaySessionId,
            StoreTakeAwayResultCallback onReady)
        {
            var webRequestString = string.Format("{0}/take-away-sessions/{1}/result", environmentConfig.API_URL, takeAwaySessionId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            UnityWebRequest webRequest = GetUnityWebRequestPOSTZip(webRequestString, fileData);
            webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            yield return webRequest.SendWebRequest();

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
                    if(environmentConfig.TurnOnLogging) Debug.LogError(":\nReceived: " + webRequest.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.Success:
                    if(environmentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }

            onReady?.Invoke(webRequest.result);
        }
    }
}
