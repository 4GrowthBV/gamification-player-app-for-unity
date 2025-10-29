using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoCreateChatPredefinedMessage(string identifier,
            string content,
            List<string> buttons,
            string buttonName,
            CreateChatPredefinedMessageCallback onReady = null,
            Guid organisationId = default,
            Guid microGameId = default)
        {
            if (organisationId == default)
            {
                sessionData.TryGetLatestOrganisationId(out organisationId);
            }

            if (microGameId == default)
            {
                sessionData.TryGetLatestMicroGameId(out microGameId);
            }

            yield return CoCreateChatPredefinedMessage(identifier, content, buttons, buttonName, organisationId, microGameId, onReady);
        }

        private IEnumerator CoCreateChatPredefinedMessage(string identifier,
            string content,
            List<string> buttons,
            string buttonName,
            Guid organisationId,
            Guid microGameId,
            CreateChatPredefinedMessageCallback onReady = null)
        {
            var createMessageDTO = CreateChatPredefinedMessageRequestDTO.Create(identifier, content, buttons, buttonName, organisationId, microGameId);
            sessionData.AddToLog(createMessageDTO.Data);

            var data = createMessageDTO.ToJson();

            var webRequestString = string.Format("{0}/chat-predefined-messages", environmentConfig.API_URL);

            if (environmentConfig.TurnOnLogging) Debug.Log(data);
            if (environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if (environmentConfig.TryGetMockDTO<CreateChatPredefinedMessageResponseDTO>(out var dto))
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

                var obj = new CreateChatPredefinedMessageResponseDTO();

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
                        obj = webRequest.downloadHandler.text.FromJson<CreateChatPredefinedMessageResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}