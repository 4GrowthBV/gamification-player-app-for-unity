using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using GamificationPlayer.Chat.Services;

namespace GamificationPlayer.TestBed.ProductionServices
{
    /// <summary>
    /// Production OpenAI API integration for ChatManager test bed
    /// Implements real OpenAI ChatGPT API calls with streaming support
    /// </summary>
    public class OpenAIChatService : MonoBehaviour, IChatAIService
    {
        [Header("OpenAI Configuration")]
        [SerializeField] private string apiKey = ""; // Set your OpenAI API key here
        [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";
        [SerializeField] private string model = "gpt-3.5-turbo";
        [SerializeField] private float temperature = 0.7f;
        [SerializeField] private int maxTokens = 500;
        [SerializeField] private float requestTimeout = 30f;

        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool simulateStreaming = true; // For testing without SSE implementation

        /// <summary>
        /// Generate AI response with streaming support (production implementation)
        /// </summary>
        public IEnumerator GenerateResponse(string message, string instruction, string examples, string knowledge, string profileContext, string conversationHistory, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            Log($"Generating OpenAI response for message: {message}");
            
            if (string.IsNullOrEmpty(apiKey))
            {
                var errorResult = AIResponseResult.Error("OpenAI API key not configured");
                onComplete?.Invoke(errorResult);
                yield break;
            }

            // Build the conversation context for OpenAI
            var requestData = BuildOpenAIRequest(message, instruction, examples, knowledge, profileContext, conversationHistory);
            
            // Create web request
            var jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (var request = new UnityWebRequest(apiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                request.timeout = (int)requestTimeout;

                Log($"Sending request to OpenAI API...");
                
                // Send request
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                        
                        if (response?.choices != null && response.choices.Length > 0)
                        {
                            string aiResponse = response.choices[0].message.content;
                            
                            // Simulate streaming if enabled (for UI testing without SSE)
                            if (simulateStreaming && onStreamChunk != null)
                            {
                                // Move streaming outside try-catch to avoid yield issue
                                StartCoroutine(SimulateStreamingResponse(aiResponse, onStreamChunk));
                            }
                            
                            // Build updated conversation history
                            string updatedHistory = $"{conversationHistory}\nUser: {message}\nAssistant: {aiResponse}";
                            
                            var result = new AIResponseResult(aiResponse, updatedHistory)
                            {
                                isStreamComplete = true
                            };
                            
                            Log($"OpenAI response received: {aiResponse.Substring(0, Math.Min(100, aiResponse.Length))}...");
                            onComplete?.Invoke(result);
                        }
                        else
                        {
                            LogError("OpenAI response format invalid - no choices found");
                            onComplete?.Invoke(AIResponseResult.Error("Invalid response format from OpenAI"));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to parse OpenAI response: {ex.Message}");
                        onComplete?.Invoke(AIResponseResult.Error($"Response parsing error: {ex.Message}"));
                    }
                }
                else
                {
                    LogError($"OpenAI API request failed: {request.error}");
                    string errorMsg = $"OpenAI API Error: {request.error}";
                    
                    // Try to get more specific error from response
                    if (!string.IsNullOrEmpty(request.downloadHandler.text))
                    {
                        try
                        {
                            var errorResponse = JsonUtility.FromJson<OpenAIErrorResponse>(request.downloadHandler.text);
                            if (errorResponse?.error != null)
                            {
                                errorMsg = $"OpenAI Error: {errorResponse.error.message}";
                            }
                        }
                        catch { /* Ignore JSON parsing errors for error responses */ }
                    }
                    
                    onComplete?.Invoke(AIResponseResult.Error(errorMsg));
                }
            }
        }

        /// <summary>
        /// Generate updated user profile (production implementation)
        /// </summary>
        public IEnumerator GenerateProfile(string newMessage, string currentProfile, string conversationHistory, string profileInstruction, Action<ProfileGenerationResult> onComplete)
        {
            Log($"Generating profile update for message: {newMessage}");
            
            if (string.IsNullOrEmpty(apiKey))
            {
                var errorResult = ProfileGenerationResult.Error("OpenAI API key not configured");
                onComplete?.Invoke(errorResult);
                yield break;
            }

            // Build profile generation request
            var requestData = BuildProfileUpdateRequest(newMessage, currentProfile, conversationHistory, profileInstruction);
            
            var jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (var request = new UnityWebRequest(apiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                request.timeout = (int)requestTimeout;

                Log($"Sending profile update request to OpenAI API...");
                
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                        
                        if (response?.choices != null && response.choices.Length > 0)
                        {
                            string updatedProfile = response.choices[0].message.content;
                            
                            var result = new ProfileGenerationResult(updatedProfile);
                            
                            Log($"Profile updated successfully: {updatedProfile.Substring(0, Math.Min(100, updatedProfile.Length))}...");
                            onComplete?.Invoke(result);
                        }
                        else
                        {
                            LogError("OpenAI profile response format invalid");
                            onComplete?.Invoke(ProfileGenerationResult.Error("Invalid response format from OpenAI"));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to parse OpenAI profile response: {ex.Message}");
                        onComplete?.Invoke(ProfileGenerationResult.Error($"Response parsing error: {ex.Message}"));
                    }
                }
                else
                {
                    LogError($"OpenAI profile API request failed: {request.error}");
                    onComplete?.Invoke(ProfileGenerationResult.Error($"OpenAI API Error: {request.error}"));
                }
            }
        }

        /// <summary>
        /// Build OpenAI API request for chat completion
        /// </summary>
        private OpenAIRequest BuildOpenAIRequest(string message, string instruction, string examples, string knowledge, string profileContext, string conversationHistory)
        {
            var messages = new List<OpenAIMessage>();
            
            // System message with agent instruction and context
            var systemContent = new StringBuilder();
            systemContent.AppendLine(instruction);
            
            if (!string.IsNullOrEmpty(examples))
            {
                systemContent.AppendLine("\nExamples:");
                systemContent.AppendLine(examples);
            }
            
            if (!string.IsNullOrEmpty(knowledge))
            {
                systemContent.AppendLine("\nKnowledge Base:");
                systemContent.AppendLine(knowledge);
            }
            
            if (!string.IsNullOrEmpty(profileContext))
            {
                systemContent.AppendLine("\nUser Profile:");
                systemContent.AppendLine(profileContext);
            }
            
            messages.Add(new OpenAIMessage("system", systemContent.ToString()));
            
            // Add conversation history if available
            if (!string.IsNullOrEmpty(conversationHistory))
            {
                // Parse conversation history into separate messages
                var historyLines = conversationHistory.Split('\n');
                foreach (var line in historyLines)
                {
                    if (line.StartsWith("User: "))
                    {
                        messages.Add(new OpenAIMessage("user", line.Substring(6)));
                    }
                    else if (line.StartsWith("Assistant: "))
                    {
                        messages.Add(new OpenAIMessage("assistant", line.Substring(11)));
                    }
                }
            }
            
            // Add current user message
            messages.Add(new OpenAIMessage("user", message));
            
            return new OpenAIRequest
            {
                model = this.model,
                messages = messages.ToArray(),
                temperature = temperature,
                max_tokens = maxTokens
            };
        }

        /// <summary>
        /// Build profile update request
        /// </summary>
        private OpenAIRequest BuildProfileUpdateRequest(string newMessage, string currentProfile, string conversationHistory, string profileInstruction)
        {
            var messages = new List<OpenAIMessage>();
            
            // System message with profile instruction
            messages.Add(new OpenAIMessage("system", profileInstruction));
            
            // Current profile context
            messages.Add(new OpenAIMessage("user", $"Current Profile:\n{currentProfile}"));
            
            // Conversation context
            if (!string.IsNullOrEmpty(conversationHistory))
            {
                messages.Add(new OpenAIMessage("user", $"Conversation History:\n{conversationHistory}"));
            }
            
            // New message to incorporate
            messages.Add(new OpenAIMessage("user", $"New Message to Process:\n{newMessage}\n\nPlease update the user profile based on this new information:"));
            
            return new OpenAIRequest
            {
                model = this.model,
                messages = messages.ToArray(),
                temperature = temperature * 0.8f, // Lower temperature for profile updates
                max_tokens = maxTokens
            };
        }

        /// <summary>
        /// Simulate streaming response for UI testing (until real SSE is implemented)
        /// </summary>
        private IEnumerator SimulateStreamingResponse(string fullResponse, Action<string> onStreamChunk)
        {
            var words = fullResponse.Split(' ');
            var streamedContent = new StringBuilder();
            
            for (int i = 0; i < words.Length; i++)
            {
                streamedContent.Append(words[i]);
                if (i < words.Length - 1) streamedContent.Append(" ");
                
                onStreamChunk?.Invoke(streamedContent.ToString());
                
                // Simulate typing delay
                yield return new WaitForSeconds(0.05f);
            }
        }

        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[OpenAIChatService] {message}");
            }
        }

        private void LogError(string message)
        {
            if (enableLogging)
            {
                Debug.LogError($"[OpenAIChatService] {message}");
            }
        }

        #region OpenAI API Data Structures

        [Serializable]
        private class OpenAIRequest
        {
            public string model;
            public OpenAIMessage[] messages;
            public float temperature;
            public int max_tokens;
        }

        [Serializable]
        private class OpenAIMessage
        {
            public string role;
            public string content;

            public OpenAIMessage(string role, string content)
            {
                this.role = role;
                this.content = content;
            }
        }

        [Serializable]
        private class OpenAIResponse
        {
            public OpenAIChoice[] choices;
        }

        [Serializable]
        private class OpenAIChoice
        {
            public OpenAIMessage message;
        }

        [Serializable]
        private class OpenAIErrorResponse
        {
            public OpenAIError error;
        }

        [Serializable]
        private class OpenAIError
        {
            public string message;
            public string type;
        }

        #endregion
    }
}