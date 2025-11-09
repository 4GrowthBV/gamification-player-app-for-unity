using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    public class ChatAIService : IChatAIService
    {
        [Serializable]
        public class OpenAIMessage
        {
            public string role;
            public string content;
        }

        [Serializable]
        public class OpenAIChatRequest
        {
            public string model;
            public OpenAIMessage[] messages;
            public int max_tokens;
            public float temperature;
            public bool stream;
        }

        [Serializable]
        public class OpenAIStreamResponse
        {
            public List<StreamChoice> choices;
            [Serializable]
            public class StreamChoice
            {
                public OpenAIMessage delta;
                public string finish_reason;
            }
        }

        [Serializable]
        public class OpenAIOneShotResponse
        {
            public List<OneShotChoice> choices;
            [Serializable]
            public class OneShotChoice
            {
                public OpenAIMessage message;
                public string finish_reason;
            }
        }

        private const string API_URL = "https://api.openai.com/v1/chat/completions";

        private readonly string apiKey;
        private readonly string model;
        private readonly int maxTokens;
        private readonly float temperature;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JS_StartOpenAIStream(string apiKey, string jsonBody);
#endif

        public ChatAIService(string apiKey,
            string model = "gpt-4.1-mini",
            int maxTokens = 500,
            float temperature = 0.7f)
        {
            this.apiKey = apiKey;
            this.model = model;
            this.maxTokens = maxTokens;
            this.temperature = temperature;
        }

        // -------------------- PUBLIC API --------------------

        public IEnumerator GenerateProfile(
            string currentProfile,
            ChatManager.ChatMessage[] conversationHistory,
            string profileInstruction,
            Action<AIResponseResult> onComplete)
        {
            var systemMsg = $"{profileInstruction}\n\nCurrent Profile:\n{currentProfile}";
            var messages = BuildMessages(systemMsg, conversationHistory);

            yield return ExecuteRequest(messages, false, null, onComplete);
        }
        
        public IEnumerator GetAIAgentName(ChatManager.ChatMessage[] conversationHistory,
            string getAIAgentNameInstruction,
            Action<AIResponseResult> onComplete)
        {
            var systemMsg = $"{getAIAgentNameInstruction}";
            var messages = BuildMessages(systemMsg, conversationHistory);

            yield return ExecuteRequest(messages, false, null, onComplete);
        }

        public IEnumerator GenerateResponse(
            string instruction,
            string examples,
            string knowledge,
            string profile,
            ChatManager.ChatMessage[] history,
            Action<string> onStreamChunk,
            Action<AIResponseResult> onComplete)
        {
            string systemMsg = $"{instruction}\n\nExamples:\n{examples}\n\nKnowledge:\n{knowledge}\n\nProfile:\n{profile}";
            var messages = BuildMessages(systemMsg, history);

#if UNITY_WEBGL && !UNITY_EDITOR
            // Use browser-based JS streaming
            yield return StartWebGLStream(messages, onStreamChunk, onComplete);
#else
            yield return ExecuteRequest(messages, true, onStreamChunk, onComplete);
#endif
        }

        // -------------------- INTERNAL CORE --------------------

        private OpenAIMessage[] BuildMessages(string systemMessage, ChatManager.ChatMessage[] history)
        {
            var list = new List<OpenAIMessage>
            {
                new OpenAIMessage { role = "developer", content = systemMessage }
            };
            foreach (var msg in history)
                list.Add(new OpenAIMessage { role = GetRole(msg), content = msg.message });
            return list.ToArray();
        }

        private string GetRole(ChatManager.ChatMessage message)
        {
            if (message.role.Contains(ChatManager.RolePrefix.user_activity.ToString()))
            {
                return "developer";
            }

            if (message.role.Contains(ChatManager.RolePrefix.user.ToString()))
            {
                return "user";
            }
            
            return "assistant";
        }

        private IEnumerator ExecuteRequest(OpenAIMessage[] messages, bool stream, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            Debug.Log("Executing OpenAI request...");
            var request = new OpenAIChatRequest
            {
                model = model,
                messages = messages,
                max_tokens = maxTokens,
                temperature = temperature,
                stream = stream
            };
            string jsonBody = JsonUtility.ToJson(request);

            if (stream)
                yield return StreamRequest(jsonBody, onStreamChunk, onComplete);
            else
                yield return OneShotRequest(jsonBody, onComplete);
        }

        // -------------------- NON-STREAMING --------------------

        private IEnumerator OneShotRequest(string jsonBody, Action<AIResponseResult> onComplete)
        {
            using (var www = new UnityWebRequest(API_URL, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + apiKey);

                Debug.Log("Sending OneShotRequest..." + jsonBody);
                yield return www.SendWebRequest();
                Debug.Log("OneShotRequest completed.");

                if (www.result != UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(AIResponseResult.Error(www.error));
                    yield break;
                }

                string text = www.downloadHandler.text;
                Debug.Log("OneShotRequest response: " + text);
                var parsed = JsonUtility.FromJson<OpenAIOneShotResponse>(text);
                string content = parsed?.choices?[0]?.message?.content ?? "";
                onComplete?.Invoke(new AIResponseResult(content));
            }
        }

        // -------------------- TRUE STREAMING (MOBILE / DESKTOP) --------------------
        private IEnumerator StreamRequest(string jsonBody, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            var task = StreamRequestAsync(jsonBody, onStreamChunk, onComplete);

            yield return new WaitUntil(() => task.IsCompleted); // Keep coroutine alive until callbacks complete
        }

        private async Task StreamRequestAsync(string jsonBody, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            var sb = new StringBuilder();
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/event-stream"));

                using (var request = new HttpRequestMessage(HttpMethod.Post, API_URL))
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    using (var response = await client.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        CancellationToken.None))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (!line.StartsWith("data: ")) continue;
                            var data = line.Substring(6).Trim();
                            if (data == "[DONE]") break;

                            var chunk = JsonUtility.FromJson<OpenAIStreamResponse>(data);
                            var delta = chunk?.choices?[0]?.delta?.content;
                            if (!string.IsNullOrEmpty(delta))
                            {
                                sb.Append(delta);
                                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                                {
                                    onStreamChunk?.Invoke(delta);
                                });
                            }
                        }
                    }
                }
            }

            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                onComplete?.Invoke(new AIResponseResult(sb.ToString()));
            });
        }

        // -------------------- WEBGL STREAM HANDLER --------------------

#if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator StartWebGLStream(OpenAIMessage[] messages, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            var body = new OpenAIChatRequest
            {
                model = model,
                messages = messages,
                max_tokens = maxTokens,
                temperature = temperature,
                stream = true
            };

            string jsonBody = JsonUtility.ToJson(body);

            // check if WebGLChatBridge exists in the scene
            if (GameObject.FindFirstObjectByType<WebGLChatBridge>() == null)
            {
                var go = new GameObject("WebGLChatBridge");
                go.AddComponent<WebGLChatBridge>();
                UnityEngine.Object.DontDestroyOnLoad(go);
            }

            var isCompleted = false;
            
            // Store callbacks for JS interop
            WebGLChatBridge.OnStreamChunk = onStreamChunk;
            WebGLChatBridge.OnStreamComplete = (AIResponseResult result) =>
            {
                isCompleted = true;
                onComplete?.Invoke(result);
            };

            JS_StartOpenAIStream(apiKey, jsonBody);

            yield return new WaitUntil(() => isCompleted);
        }
#endif
    }
}
