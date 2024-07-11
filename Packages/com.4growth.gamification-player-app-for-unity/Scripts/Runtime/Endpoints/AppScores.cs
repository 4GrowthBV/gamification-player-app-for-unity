using System;
using System.Collections;
using GamificationPlayer.DTO.AppScores;
using GamificationPlayer.DTO.ExternalEvents;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoAppScores(DateTime started, 
            DateTime now, 
            int score, 
            bool isCompleted, 
            AppScoresCallback onReady = null, 
            MicroGamePayload.Integration integration = null)
        {
            AppScoresRequestDTO appScoresRequestDTO;
            DateTime? completedAt = null;
            if(isCompleted)
            {
                completedAt = now;
            }

            sessionData.TryGetLatestModuleSessionId(out var moduleSessionId);
            sessionData.TryGetLatestBattleSessionId(out var battleSessionId);

            sessionData.TryGetLatestUserId(out var userId);
            sessionData.TryGetLatestOrganisationId(out var organisationId);
            sessionData.TryGetLatestMicroGameId(out var microGameId);

            if(moduleSessionId != null && moduleSessionId != Guid.Empty)
            {
                appScoresRequestDTO = AppScoresRequestDTO.GetAppScoresModuleRequest(started, 
                    now, 
                    score, 
                    moduleSessionId, 
                    completedAt, 
                    integration);
            } 
            else if(battleSessionId != null && battleSessionId != Guid.Empty)
            {
                appScoresRequestDTO = AppScoresRequestDTO.GetAppScoresBattleRequest(started, 
                    now, 
                    score, 
                    userId, 
                    battleSessionId, 
                    completedAt, 
                    integration);
            } else
            {
                appScoresRequestDTO = AppScoresRequestDTO.GetAppScoresRequest(started, 
                    now,
                    score, 
                    userId, 
                    organisationId, 
                    microGameId, 
                    completedAt, 
                    integration);
            }            

            yield return CoAppScores(appScoresRequestDTO, onReady);
        }
        
        private IEnumerator CoAppScores(AppScoresRequestDTO appScoresRequestDTO, AppScoresCallback onReady = null)
        {
            sessionData.AddToLog(appScoresRequestDTO.data);

            string data = appScoresRequestDTO.ToJson();

            string webRequestString = string.Format("{0}/app-scores", environmentConfig.API_URL);

            if(environmentConfig.TurnOnLogging) Debug.Log(data);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<AppScoresRespondDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success, string.Empty);
            }
            else
            {
                UnityWebRequest webRequest = GetUnityWebRequestPOST(webRequestString, data);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                AppScoresRespondDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<AppScoresRespondDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                var gotoPageUrl = obj?.data?.links != null ? obj.data.links.show : string.Empty;
                onReady?.Invoke(webRequest.result, gotoPageUrl);
            }
        }
    }
}
