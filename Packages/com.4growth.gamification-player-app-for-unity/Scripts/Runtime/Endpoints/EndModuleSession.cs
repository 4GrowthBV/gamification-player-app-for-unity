using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ModuleSession;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoEndModuleSession(DateTime now, int score, bool isCompleted, EndModuleSessionCallback onReady = null)
        {
            if(!sessionData.TryGetLatestModuleSessionId(out var moduleSessionId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError);
                yield break;
            }

            yield return CoEndModuleSession(now, moduleSessionId, score, isCompleted, onReady);
        }

        private IEnumerator CoEndModuleSession(DateTime now, Guid moduleSessionId, int score, bool isCompleted, EndModuleSessionCallback onReady = null)
        {
            DateTime? completedAt = null;
            if(isCompleted)
            {
                completedAt = now;
            }

            var moduleSession = new UpdateModuleSessionRequestDTO(now, score, completedAt);
            sessionData.AddToLog(moduleSession.data);

            string data = moduleSession.ToJson();

            string webRequestString = string.Format("{0}/module-sessions/{1}?_method=PATCH", environmentConfig.API_URL, moduleSessionId);

            if(environmentConfig.TurnOnLogging) Debug.Log(data);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<UpdateModuleSessionResponseDTO>(out var dto))
            {
                Debug.Log("Mocking");
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success);
            }
            else
            {
                UnityWebRequest webRequest = GetUnityWebRequestPOST(webRequestString, data);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
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
                        var obj = webRequest.downloadHandler.text.FromJson<UpdateModuleSessionResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result);
            }
        }
    }
}
