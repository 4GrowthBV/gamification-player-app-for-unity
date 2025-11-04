using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer.Chat.Services
{
    /// <summary>
    /// Example implementation of IChatRouterService using n8n workflow
    /// This would typically be in a separate package/assembly
    /// </summary>
    public class N8nRouterService : MonoBehaviour, IChatRouterService
    {
        [SerializeField] private string n8nEndpoint = "https://your-n8n-instance.com/webhook/chat-router";
        
        public IEnumerator RouteMessage(string userMessage, string conversationHistory, Action<RouterResult> onComplete)
        {
            var requestData = new
            {
                message = userMessage,
                conversationHistory = conversationHistory
            };
            
            string jsonData = JsonUtility.ToJson(requestData);
            
            using (UnityWebRequest request = new UnityWebRequest(n8nEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<N8nRouterResponse>(request.downloadHandler.text);
                        var result = new RouterResult(response.agent, response.examples, response.knowledge);
                        onComplete?.Invoke(result);
                    }
                    catch (Exception e)
                    {
                        onComplete?.Invoke(RouterResult.Error($"Failed to parse n8n response: {e.Message}"));
                    }
                }
                else
                {
                    onComplete?.Invoke(RouterResult.Error($"n8n request failed: {request.error}"));
                }
            }
        }
        
        [Serializable]
        private class N8nRouterResponse
        {
            public string agent;
            public string examples;
            public string knowledge;
        }
    }
}