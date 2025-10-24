using System;
using System.Collections;
using GamificationPlayer.DTO.MicroGame;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetMicroGames(GetMicroGamesCallback onReady = null, Guid organisationId = default)
        {
            if (organisationId == default)
            {
                sessionData.TryGetLatestOrganisationId(out organisationId);
            }

            yield return CoGetMicroGames(organisationId, onReady);
        }

        private IEnumerator CoGetMicroGames(Guid organisationId, GetMicroGamesCallback onReady = null)
        {
            var url = string.Format("{0}/organisations/{1}/micro-games", environmentConfig.API_URL, organisationId);

            if (environmentConfig.TurnOnLogging) Debug.Log(url);

            if (environmentConfig.TryGetMockDTO<GetMicroGamesResponseDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success, dto);
            }
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(url);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                GetMicroGamesResponseDTO obj = null;

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
                        if (environmentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                        obj = webRequest.downloadHandler.text.FromJson<GetMicroGamesResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}
