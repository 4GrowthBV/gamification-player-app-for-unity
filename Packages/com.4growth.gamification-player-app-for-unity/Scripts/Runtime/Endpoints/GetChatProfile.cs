using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetChatProfile(Guid chatProfileId, GetChatProfileCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/chat-profiles/{1}", environmentConfig.API_URL, chatProfileId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.TryGetMockDTO<GetChatProfileResponseDTO>(out var dto))
            {
                sessionData.AddToLog(dto.data, false);
                onReady?.Invoke(UnityWebRequest.Result.Success, dto);
            } 
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(webRequestString);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                GetChatProfileResponseDTO obj = null;

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
                        
                        var chatProfile = webRequest.downloadHandler.text.FromJson<GetChatProfileResponseDTO>();

                        if (chatProfile != null)
                        {
                            obj = chatProfile;
                            sessionData.AddToLog(obj.data, false);
                        }
                        else
                        {
                            Debug.LogError("Error parsing chat profile response.");
                        }

                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}