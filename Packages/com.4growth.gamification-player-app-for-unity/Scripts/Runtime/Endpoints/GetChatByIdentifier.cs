using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        /// <summary>
        /// Get a chat predefined message by identifier
        /// </summary>
        /// <param name="identifier">The identifier of the predefined message</param>
        /// <param name="onReady">Callback when request completes</param>
        /// <returns></returns>
        public IEnumerator CoGetChatPredefinedMessageByIdentifier(string identifier, GetChatPredefinedMessageCallback onReady = null, Guid organisationId = default, Guid userId = default, Guid microGameId = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                Debug.LogError("Identifier cannot be null or empty");
                onReady?.Invoke(UnityWebRequest.Result.DataProcessingError, null);
                yield break;
            }

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

            var url = string.Format("{0}/chat-predefined-messages?identifier={1}&organisation={2}&user={3}&micro_game={4}",
                environmentConfig.API_URL, identifier, organisationId, userId, microGameId);

            if (environmentConfig.TurnOnLogging) Debug.Log(url);

            if (environmentConfig.TryGetMockDTO<GetChatPredefinedMessageResponseDTO>(out var dto))
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

                GetChatPredefinedMessageResponseDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<GetChatPredefinedMessageResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }

        /// <summary>
        /// Get chat instruction by agent name
        /// </summary>
        /// <param name="agent">The agent name</param>
        /// <param name="onReady">Callback when request completes</param>
        /// <returns></returns>
        public IEnumerator CoGetChatInstructionByAgent(string agent, GetChatInstructionCallback onReady = null, Guid organisationId = default, Guid userId = default, Guid microGameId = default)
        {
            if (string.IsNullOrEmpty(agent))
            {
                Debug.LogError("Agent cannot be null or empty");
                onReady?.Invoke(UnityWebRequest.Result.DataProcessingError, null);
                yield break;
            }

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

            var url = string.Format("{0}/chat-instructions?agent={1}&organisation={2}&user={3}&micro_game={4}",
                environmentConfig.API_URL, agent, organisationId, userId, microGameId);

            if (environmentConfig.TurnOnLogging) Debug.Log(url);

            if (environmentConfig.TryGetMockDTO<GetChatInstructionResponseDTO>(out var dto))
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

                GetChatInstructionResponseDTO obj = null;

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
                        obj = webRequest.downloadHandler.text.FromJson<GetChatInstructionResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}