using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetChatConversationMessages(GetChatConversationMessagesCallback onReady = null,
            Guid chatConversationId = default,
            int page = 1,
            int perPage = 25)
        {
            yield return CoGetChatConversationMessages(chatConversationId, page, perPage, onReady);
        }

        private IEnumerator CoGetChatConversationMessages(Guid chatConversationId,
            int page,
            int perPage,
            GetChatConversationMessagesCallback onReady = null)
        {
            var url = string.Format("{0}/chat-conversation-messages?chat_conversation={1}&page={2}&per_page={3}",
                environmentConfig.API_URL, chatConversationId, page, perPage);

            if (environmentConfig.TurnOnLogging) Debug.Log(url);

            if (environmentConfig.TryGetMockDTO<GetChatConversationMessagesResponseDTO>(out var dto))
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

                GetChatConversationMessagesResponseDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<GetChatConversationMessagesResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}