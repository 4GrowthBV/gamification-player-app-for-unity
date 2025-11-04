using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoGetChatProfile(Guid chatConversationId, GetChatProfileCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/chat-conversations/{1}", environmentConfig.API_URL, chatConversationId);

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
                        
                        var chatConversation = webRequest.downloadHandler.text.FromJson<GetChatConversationResponseDTO>();

                        foreach (var includedItem in chatConversation.included)
                        {
                            if (includedItem.Type == "chat_profile")
                            {
                                obj = new GetChatProfileResponseDTO
                                {
                                    data = new GetChatProfileResponseDTO.Data
                                    {
                                        id = includedItem.id,
                                        type = includedItem.type,
                                        attributes = new GetChatProfileResponseDTO.ProfileAttributes
                                        {
                                            profile = includedItem.attributes.ContainsKey("profile") ? includedItem.attributes["profile"].ToString() : null,
                                            created_at = includedItem.attributes.ContainsKey("created_at") ? includedItem.attributes["created_at"].ToString() : null,
                                            updated_at = includedItem.attributes.ContainsKey("updated_at") ? includedItem.attributes["updated_at"].ToString() : null
                                        }
                                    },
                                };

                                break;
                            }
                        }

                        if (obj != null)
                        {
                            sessionData.AddToLog(obj.data, false);
                        }

                        break;
                }

                onReady?.Invoke(webRequest.result, obj);
            }
        }
    }
}