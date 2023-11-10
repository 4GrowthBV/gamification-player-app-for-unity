using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetUserStatistics(GetUserStatisticsCallback onReady = null)
        {
            if(!sessionData.TryGetLatestUserId(out var userId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError);

                yield break;
            }

            if(!sessionData.TryGetLatestOrganisationId(out var organisationId))
            {   
                onReady?.Invoke(UnityWebRequest.Result.ProtocolError);

                yield break;
            }

            yield return CoGetUserStatistics(userId, organisationId, onReady);
        }

        private IEnumerator CoGetUserStatistics(Guid userId, Guid organisationId, GetUserStatisticsCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/organisations/{1}/users/{2}/statistics", environmentConfig.API_URL, organisationId, userId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<GetUserStatisticsDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success);
            } 
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                GetUserStatisticsDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<GetUserStatisticsDTO>();
                        sessionData.AddToLog(obj.data, false);           
                        break;
                }

                onReady?.Invoke(webRequest.result);
            }
        }   
    }
}
