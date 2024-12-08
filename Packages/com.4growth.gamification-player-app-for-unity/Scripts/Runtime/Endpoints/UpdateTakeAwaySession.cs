using System;
using System.Collections;
using GamificationPlayer.DTO.TakeAway;
using GamificationPlayer.Session;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoUpdateTakeAwaySessions(DateTime startedAt,
            DateTime endedAt,
            UpdateTakeAwaySessionCallback onReady = null, 
            Guid takeAwaySessionId = default)
        {
            if(takeAwaySessionId == default)
            {
                sessionData.TryGetLatestId<TakeAwaySessionId>(out takeAwaySessionId);
            }

            yield return CoUpdateTakeAwaySessions(takeAwaySessionId, startedAt, endedAt, onReady);
        }

        private IEnumerator CoUpdateTakeAwaySessions(Guid takeAwaySessionId,
            DateTime startedAt,
            DateTime endedAt,
            UpdateTakeAwaySessionCallback onReady)
        {
            var now = DateTime.Now;
            var takeAwayDTO = new UpdateTakeAwaySessionRequestDTO(startedAt, endedAt);
            sessionData.AddToLog(takeAwayDTO.data);

            var data = takeAwayDTO.ToJson();

            var webRequestString = string.Format("{0}/take-away-sessions/{1}", environmentConfig.API_URL, takeAwaySessionId);

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
