using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoCreateChatConversation(CreateChatConversationCallback onReady = null,
            Guid organisationId = default,
            Guid userId = default,
            Guid microGameId = default)
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

            yield return CoCreateChatConversation(organisationId, userId, microGameId, onReady);
        }

        private IEnumerator CoCreateChatConversation(Guid organisationId,
            Guid userId,
            Guid microGameId,
            CreateChatConversationCallback onReady = null)
        {
            var createChatConversationDTO = CreateChatConversationRequestDTO.Create(organisationId, userId, microGameId);
            sessionData.AddToLog(createChatConversationDTO.Data);

            var data = createChatConversationDTO.ToJson();

            var webRequestString = string.Format("{0}/chat-conversations", environmentConfig.API_URL);

            if (environmentConfig.TurnOnLogging) Debug.Log(data);
            if (environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if (environmentConfig.TryGetMockDTO<CreateChatConversationResponseDTO>(out var dto))
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

                var obj = new CreateChatConversationResponseDTO();

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
                        obj = webRequest.downloadHandler.text.FromJson<CreateChatConversationResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}