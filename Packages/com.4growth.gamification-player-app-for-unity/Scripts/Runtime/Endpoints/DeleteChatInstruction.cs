using System;
using System.Collections;
using GamificationPlayer.DTO.Chat;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public partial class GamificationPlayerEndpoints
    {
        public IEnumerator CoDeleteChatInstruction(Guid chatInstructionId, DeleteChatInstructionCallback onReady = null)
        {
            string webRequestString = string.Format("{0}/chat-instructions/{1}", environmentConfig.API_URL, chatInstructionId);

            if(environmentConfig.TurnOnLogging) Debug.Log(webRequestString);

            if(environmentConfig.IsMockServer)
            {
                onReady?.Invoke(UnityWebRequest.Result.Success);
            } 
            else
            {
                UnityWebRequest webRequest = UnityWebRequest.Delete(webRequestString);
                webRequest.SetRequestHeader("X-Api-Key", environmentConfig.APIKey);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

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
                        if(environmentConfig.TurnOnLogging) Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                        break;
                }

                onReady?.Invoke(webRequest.result);
            }
        }
    }
}
