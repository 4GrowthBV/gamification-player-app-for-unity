using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoUpdateChatProfile(Guid chatProfileId,
            string profile,
            UpdateChatProfileCallback onReady = null)
        {
            var updateDTO = UpdateChatProfileRequestDTO.Create(profile);
            sessionData.AddToLog(updateDTO.Data);

            var data = updateDTO.ToJson();
            var webRequestString = string.Format("{0}/chat-profiles/{1}", environmentConfig.API_URL, chatProfileId);

            if (environmentConfig.TurnOnLogging) Debug.Log(data);
            if (environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if (environmentConfig.TryGetMockDTO<UpdateChatProfileResponseDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success, dto);
            }
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Put(webRequestString, data);
                webRequest.method = "PATCH";
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                var obj = new UpdateChatProfileResponseDTO();

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
                        obj = webRequest.downloadHandler.text.FromJson<UpdateChatProfileResponseDTO>();
                        sessionData.AddToLog(obj.data, false);
                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}