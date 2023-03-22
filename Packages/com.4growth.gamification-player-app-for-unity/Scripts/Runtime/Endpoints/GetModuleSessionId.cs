using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GamificationPlayer.DTO.ModuleSession;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetModuleSessionId(GetModuleSessionIdCallback onReady)
        {
            if(!sessionData.TryGetLatestChallengeSessionId(out var challengeSessionId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError, Guid.Empty);

                yield break;
            }

            if(!sessionData.TryGetLatestModuleId(out var moduleId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError, Guid.Empty);

                yield break;
            }

            yield return CoGetModuleSessionId(challengeSessionId, moduleId, onReady);
        }

        private IEnumerator CoGetModuleSessionId(Guid challengeSessionId, Guid moduleId, GetModuleSessionIdCallback onReady)
        {
            string webRequestString = string.Format("{0}/module-sessions?filter%5Bmodule_id%5D={1}&filter%5Bchallenge_session_id%5D={2}", environmentConfig.API_URL, moduleId, challengeSessionId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
            webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            yield return webRequest.SendWebRequest();

            var moduleSessionId = Guid.Empty;

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
                    var obj = webRequest.downloadHandler.text.FromJson<GetModuleSessionsResponseDTO>();
                    sessionData.AddToLog(obj.data);
                    moduleSessionId = new Guid(obj.data.First().id);
                    break;
            }

            onReady?.Invoke(webRequest.result, moduleSessionId);
        }
    }
}
