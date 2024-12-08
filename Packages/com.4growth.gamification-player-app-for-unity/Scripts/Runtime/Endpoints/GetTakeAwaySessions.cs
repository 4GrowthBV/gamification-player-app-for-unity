using System;
using System.Collections;
using GamificationPlayer.DTO.TakeAway;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetTakeAwaySessions(GetTakeAwaySessionCallback onReady = null, 
            bool isModuleSession = false,
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

            if(isModuleSession && moduleSessionId == default)
            {
                sessionData.TryGetLatestModuleSessionId(out moduleSessionId);
            }

            yield return CoGetTakeAwaySession(microGameId, userId, organisationId, onReady, moduleSessionId);
        }

        private IEnumerator CoGetTakeAwaySession(Guid microGameId,
            Guid userId,
            Guid organisationId,
            GetTakeAwaySessionCallback onReady,
            Guid? moduleSessionId = null)
        {
            var takeAwayDTO = new GetTakeAwaySessionsRequestDTO(microGameId, userId, organisationId, moduleSessionId);

            var data = takeAwayDTO.ToJson();

            var webRequestString = string.Format("{0}/take-away-sessions", environmentConfig.API_URL, organisationId);

            if(environmentConfig.TurnOnLogging) Debug.Log(data);
            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<GetTakeAwaySessionsResponseDTO>(out var dto))
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

                var obj = new GetTakeAwaySessionsResponseDTO();

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
                        obj = webRequest.downloadHandler.text.FromJson<GetTakeAwaySessionsResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}
