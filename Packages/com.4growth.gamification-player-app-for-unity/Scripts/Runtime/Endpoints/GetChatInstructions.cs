using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetChatInstructions(GetChatInstructionsCallback onReady = null,
            Guid organisationId = default,
            Guid userId = default,
            Guid microGameId = default,
            int page = 1,
            int perPage = 25)
        {
            if (organisationId == default)
            {
                sessionData.TryGetLatestOrganisationId(out organisationId);
            }

            if (userId == default)
            {
                sessionData.TryGetLatestUserId(out userId);
            }

            if (microGameId == default)
            {
                sessionData.TryGetLatestMicroGameId(out microGameId);
            }

            yield return CoGetChatInstructions(organisationId, userId, microGameId, page, perPage, onReady);
        }

        private IEnumerator CoGetChatInstructions(Guid organisationId,
            Guid userId,
            Guid microGameId,
            int page,
            int perPage,
            GetChatInstructionsCallback onReady = null)
        {
            var url = string.Format("{0}/chat-instructions?organisation={1}&user={2}&micro_game={3}&page={4}&per_page={5}",
                environmentConfig.API_URL, organisationId, userId, microGameId, page, perPage);

            if (environmentConfig.TurnOnLogging) Debug.Log(url);

            if (environmentConfig.TryGetMockDTO<GetChatInstructionsResponseDTO>(out var dto))
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

                GetChatInstructionsResponseDTO obj = null;

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
                        if (environmentConfig.TurnOnLogging) Debug.LogError(":\nReceived: " + webRequest.downloadHandler.text);
                        break;
                    case UnityWebRequest.Result.Success:
                        if (environmentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                        obj = webRequest.downloadHandler.text.FromJson<GetChatInstructionsResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}