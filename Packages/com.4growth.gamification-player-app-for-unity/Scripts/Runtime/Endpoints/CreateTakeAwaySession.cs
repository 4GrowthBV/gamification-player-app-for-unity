using System;
using System.Collections;
using GamificationPlayer.DTO.TakeAway;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoCreateTakeAwaySession(DateTime started, 
            DateTime ended,
            CreateTakeAwaySessionCallback onReady = null, 
            Guid microGameId = default,
            Guid userId = default,
            Guid organisationId = default,
            Guid moduleSessionId = default)
        {
            if(microGameId == default)
            {
                sessionData.TryGetLatestMicroGameId(out microGameId);
            }

            if(userId == default)
            {
                sessionData.TryGetLatestUserId(out userId);
            }

            if(organisationId == default)
            {
                sessionData.TryGetLatestOrganisationId(out organisationId);
            }

            if(moduleSessionId == default)
            {
                sessionData.TryGetLatestModuleSessionId(out moduleSessionId);
            }

            yield return CoCreateTakeAwaySession(started, ended, microGameId, userId, organisationId, onReady, moduleSessionId == default ? null : moduleSessionId);
        }

        private IEnumerator CoCreateTakeAwaySession(DateTime started, 
            DateTime ended,
            Guid microGameId,
            Guid userId,
            Guid organisationId,
            CreateTakeAwaySessionCallback onReady,
            Guid? moduleSessionId = null)
        {
            Debug.Log("CoCreateTakeAwaySession: " + moduleSessionId);
            var takeAwayDTO = new CreateTakeAwaySessionRequestDTO(started, ended, microGameId, userId, organisationId, moduleSessionId);
            sessionData.AddToLog(takeAwayDTO.data);

            var data = takeAwayDTO.ToJson();

            var webRequestString = string.Format("{0}/take-away-sessions", environmentConfig.API_URL, organisationId);

            if(environmentConfig.TurnOnLogging) Debug.Log(data);
            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<TakeAwaySessionResponseDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success, dto);
            } 
            else
            {
                UnityWebRequest webRequest = GetUnityWebRequestPOST(webRequestString, data);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                var obj = new TakeAwaySessionResponseDTO();

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
                        obj = webRequest.downloadHandler.text.FromJson<TakeAwaySessionResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}
