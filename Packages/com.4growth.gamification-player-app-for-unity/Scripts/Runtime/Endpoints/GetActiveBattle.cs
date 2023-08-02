using System;
using System.Collections;
using GamificationPlayer.DTO.Battle;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetActiveBattle(GetActiveBattleCallback onReady = null, Guid organisationId = default)
        {
            if(organisationId == default)
            {
                sessionData.TryGetLatestOrganisationId(out organisationId);
            }

            yield return CoGetActiveBattle(organisationId, onReady);
        }

        private IEnumerator CoGetActiveBattle(Guid organisationId, GetActiveBattleCallback onReady)
        {
            var url = string.Format("{0}/organisations/{1}/battles/active", environmentConfig.API_URL, organisationId);

            if(environmentConfig.TurnOnLogging) Debug.Log(url);

            if(environmentConfig.TryGetMockDTO<ActiveBattleDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success);
            } 
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(url);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                var token = string.Empty;

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
                        var userDataRoot = webRequest.downloadHandler.text.FromJson<ActiveBattleDTO>();
                        sessionData.AddToLog(userDataRoot.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result);
            }
        }
    }
}
