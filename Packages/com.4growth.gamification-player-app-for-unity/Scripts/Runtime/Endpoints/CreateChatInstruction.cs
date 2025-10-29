using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoCreateChatInstruction(string identifier,
            string instruction,
            CreateChatInstructionCallback onReady = null,
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

            yield return CoCreateChatInstruction(identifier, instruction, organisationId, microGameId, onReady);
        }

        private IEnumerator CoCreateChatInstruction(string identifier,
            string instruction,
            Guid organisationId,
            Guid microGameId,
            CreateChatInstructionCallback onReady = null)
        {
            var createInstructionDTO = CreateChatInstructionRequestDTO.Create(identifier, instruction, organisationId, microGameId);
            sessionData.AddToLog(createInstructionDTO.Data);

            var data = createInstructionDTO.ToJson();

            var webRequestString = string.Format("{0}/chat-instructions", environmentConfig.API_URL);

            if (environmentConfig.TurnOnLogging) Debug.Log(data);
            if (environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if (environmentConfig.TryGetMockDTO<CreateChatInstructionResponseDTO>(out var dto))
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

                var obj = new CreateChatInstructionResponseDTO();

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
                        obj = webRequest.downloadHandler.text.FromJson<CreateChatInstructionResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}