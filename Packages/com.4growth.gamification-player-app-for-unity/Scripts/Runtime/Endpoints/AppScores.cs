using System;
using System.Collections;
using GamificationPlayer.DTO.AppScores;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoAppScores(DateTime now, int score, bool isCompleted, AppScoresCallback onReady = null)
        {
            sessionData.TryGetLatestModuleSessionId(out var moduleSessionId);
            sessionData.TryGetLatestBattleSessionId(out var battleSessionId);
            sessionData.TryGetLatestUserId(out var userId);
            sessionData.TryGetLatestOrganisationId(out var organisationId);
            sessionData.TryGetLatestMicroGameIdentifier(out var microGameId);

            yield return CoAppScores(now, moduleSessionId, battleSessionId, userId, organisationId, microGameId, score, isCompleted, onReady);
        }
        
        private IEnumerator CoAppScores(DateTime now, 
            Guid moduleSessionId,
            Guid battleSessionId,
            Guid userId,
            Guid organisationId,
            string microGameId,
            int score, 
            bool isCompleted, 
            AppScoresCallback onReady = null)
        {
            DateTime? completedAt = null;
            if(isCompleted)
            {
                completedAt = now;
            }

            var moduleSession = new AppScoresRequestDTO(now, score, moduleSessionId, battleSessionId, userId, organisationId, microGameId, completedAt);
            sessionData.AddToLog(moduleSession.data);

            string data = moduleSession.ToJson();

            string webRequestString = string.Format("{0}/app-scores", environmentConfig.API_URL);

            if(environmentConfig.TurnOnLogging) Debug.Log(data);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<AppScoresRespondDTO>(out var dto))
            {
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
                        var obj = webRequest.downloadHandler.text.FromJson<AppScoresRespondDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result);
            }
        }
    }
}
