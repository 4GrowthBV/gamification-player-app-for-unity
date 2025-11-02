using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer.AI
{
    /// <summary>
    /// Client for OpenAI API integration
    /// Handles chat completions and streaming responses
    /// </summary>
    public class OpenAIClient
    {
        #region Configuration
        public class Config
        {
            public string apiKey;
            public string model = "gpt-4o-mini";
            public float temperature = 0.7f;
            public int maxTokens = 1000;
            public string baseUrl = "https://api.openai.com/v1";
            
            public bool IsValid()
            {
                return !string.IsNullOrEmpty(apiKey);
            }
        }
        #endregion

        #region Data Structures
        [Serializable]
        public class ChatCompletionRequest
        {
            public string model;
            public Message[] messages;
            public float temperature;
            public int max_tokens;
            public bool stream;
        }

        [Serializable]
        public class Message
        {
            public string role; // "system", "user", "assistant"
            public string content;
            
            public Message(string role, string content)
            {
                this.role = role;
                this.content = content;
            }
        }

        [Serializable]
        public class ChatCompletionResponse
        {
            public string id;
            public string @object;
            public long created;
            public string model;
            public Choice[] choices;
            public Usage usage;
        }

        [Serializable]
        public class Choice
        {
            public int index;
            public Message message;
            public string finish_reason;
        }

        [Serializable]
        public class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }

        [Serializable]
        public class ErrorResponse
        {
            public Error error;
        }

        [Serializable]
        public class Error
        {
            public string message;
            public string type;
            public string param;
            public string code;
        }
        #endregion

        #region Events
        public static event Action<string> OnResponseReceived;
        public static event Action<string> OnErrorOccurred;
        public static event Action<int> OnTokensUsed; // total tokens
        #endregion

        private Config config;
        private MonoBehaviour coroutineRunner;

        public OpenAIClient(Config config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Send a chat completion request to OpenAI
        /// </summary>
        /// <param name="messages">Conversation messages</param>
        /// <param name="onComplete">Callback with response or error</param>
        public void SendChatCompletion(List<Message> messages, System.Action<string, bool> onComplete)
        {
            if (!config.IsValid())
            {
                string error = "OpenAI API key not configured";
                OnErrorOccurred?.Invoke(error);
                onComplete?.Invoke(error, false);
                return;
            }

            coroutineRunner.StartCoroutine(SendChatCompletionCoroutine(messages, onComplete));
        }

        private IEnumerator SendChatCompletionCoroutine(List<Message> messages, System.Action<string, bool> onComplete)
        {
            var request = new ChatCompletionRequest
            {
                model = config.model,
                messages = messages.ToArray(),
                temperature = config.temperature,
                max_tokens = config.maxTokens,
                stream = false
            };

            string jsonData = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest webRequest = new UnityWebRequest($"{config.baseUrl}/chat/completions", "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ChatCompletionResponse>(webRequest.downloadHandler.text);
                        
                        if (response.choices != null && response.choices.Length > 0)
                        {
                            string content = response.choices[0].message.content;
                            
                            // Fire events
                            OnResponseReceived?.Invoke(content);
                            if (response.usage != null)
                            {
                                OnTokensUsed?.Invoke(response.usage.total_tokens);
                            }
                            
                            onComplete?.Invoke(content, true);
                        }
                        else
                        {
                            string error = "No response choices received from OpenAI";
                            OnErrorOccurred?.Invoke(error);
                            onComplete?.Invoke(error, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = $"Failed to parse OpenAI response: {ex.Message}";
                        OnErrorOccurred?.Invoke(error);
                        onComplete?.Invoke(error, false);
                    }
                }
                else
                {
                    string errorMessage = $"OpenAI API Error: {webRequest.error}";
                    
                    // Try to parse error response
                    if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {
                        try
                        {
                            var errorResponse = JsonUtility.FromJson<ErrorResponse>(webRequest.downloadHandler.text);
                            if (errorResponse.error != null)
                            {
                                errorMessage = $"OpenAI API Error: {errorResponse.error.message}";
                            }
                        }
                        catch
                        {
                            // Use original error message if parsing fails
                        }
                    }
                    
                    OnErrorOccurred?.Invoke(errorMessage);
                    onComplete?.Invoke(errorMessage, false);
                }
            }
        }

        /// <summary>
        /// Create a system message for the conversation
        /// </summary>
        public static Message CreateSystemMessage(string content)
        {
            return new Message("system", content);
        }

        /// <summary>
        /// Create a user message for the conversation
        /// </summary>
        public static Message CreateUserMessage(string content)
        {
            return new Message("user", content);
        }

        /// <summary>
        /// Create an assistant message for the conversation
        /// </summary>
        public static Message CreateAssistantMessage(string content)
        {
            return new Message("assistant", content);
        }
    }
}