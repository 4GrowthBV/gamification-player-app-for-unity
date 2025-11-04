using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer.Chat.Services
{
    /// <summary>
    /// Example implementation of IChatAIService using OpenAI API
    /// This would typically be in a separate package/assembly
    /// </summary>
    public class OpenAIChatService : MonoBehaviour, IChatAIService
    {
        [SerializeField] private string openAIApiKey = "your-openai-api-key";
        [SerializeField] private string openAIEndpoint = "https://api.openai.com/v1/chat/completions";
        
        /// <summary>
        /// Generate AI response with streaming support using OpenAI API
        /// </summary>
        public IEnumerator GenerateResponse(string message, string instruction, string examples, string knowledge, string profileContext, string conversationHistory, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            var systemMessage = BuildSystemMessage(instruction, examples, knowledge, profileContext);
            var messages = BuildMessagesWithHistory(systemMessage, message, conversationHistory);
            
            var requestData = new
            {
                model = "gpt-4",
                messages = messages,
                max_tokens = 1000,
                temperature = 0.7,
                stream = onStreamChunk != null // Enable streaming if callback provided
            };
            
            string jsonData = JsonUtility.ToJson(requestData);
            
            using (UnityWebRequest request = new UnityWebRequest(openAIEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {openAIApiKey}");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (onStreamChunk != null)
                    {
                        // Handle streaming response (simplified - real implementation would parse SSE)
                        yield return StartCoroutine(HandleStreamingResponse(request.downloadHandler.text, onStreamChunk, onComplete));
                    }
                    else
                    {
                        // Handle non-streaming response
                        var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                        if (response?.choices != null && response.choices.Length > 0)
                        {
                            var aiResponse = response.choices[0].message.content;
                            var result = new AIResponseResult(aiResponse, BuildUpdatedHistory(conversationHistory, message, aiResponse))
                            {
                                isStreamComplete = true
                            };
                            onComplete?.Invoke(result);
                        }
                        else
                        {
                            onComplete?.Invoke(AIResponseResult.Error("Invalid response format from OpenAI"));
                        }
                    }
                }
                else
                {
                    onComplete?.Invoke(AIResponseResult.Error($"OpenAI request failed: {request.error}"));
                }
            }
        }

        /// <summary>
        /// Generate updated user profile using OpenAI API
        /// </summary>
        public IEnumerator GenerateProfile(string newMessage, string currentProfile, string conversationHistory, string profileInstruction, Action<ProfileGenerationResult> onComplete)
        {
            var systemMessage = BuildProfileSystemMessage(profileInstruction);
            var userPrompt = BuildProfilePrompt(newMessage, currentProfile, conversationHistory);
            
            var requestData = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 500,
                temperature = 0.3 // Lower temperature for more consistent profile updates
            };
            
            string jsonData = JsonUtility.ToJson(requestData);
            
            using (UnityWebRequest request = new UnityWebRequest(openAIEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {openAIApiKey}");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                        var updatedProfile = response.choices[0].message.content;
                        var result = new ProfileGenerationResult(updatedProfile);
                        onComplete?.Invoke(result);
                    }
                    catch (Exception e)
                    {
                        onComplete?.Invoke(ProfileGenerationResult.Error($"Failed to parse profile response: {e.Message}"));
                    }
                }
                else
                {
                    onComplete?.Invoke(ProfileGenerationResult.Error($"Profile generation request failed: {request.error}"));
                }
            }
        }
        
        private string BuildSystemMessage(string instruction, string examples, string knowledge, string profileContext)
        {
            return $"{instruction}\n\nKnowledge: {knowledge}\n\nExamples: {examples}\n\nUser Profile Context: {profileContext}";
        }

        private object[] BuildMessagesWithHistory(string systemMessage, string userMessage, string conversationHistory)
        {
            var messages = new System.Collections.Generic.List<object>();
            
            // Add system message
            messages.Add(new { role = "system", content = systemMessage });
            
            // Add conversation history if available
            if (!string.IsNullOrEmpty(conversationHistory))
            {
                // Parse conversation history and add as context
                // For simplicity, add as single assistant message - in real implementation, parse properly
                messages.Add(new { role = "assistant", content = $"Previous conversation context: {conversationHistory}" });
            }
            
            // Add current user message
            messages.Add(new { role = "user", content = userMessage });
            
            return messages.ToArray();
        }

        private string BuildUpdatedHistory(string conversationHistory, string userMessage, string aiResponse)
        {
            var updatedHistory = conversationHistory;
            if (!string.IsNullOrEmpty(updatedHistory))
            {
                updatedHistory += "\n";
            }
            updatedHistory += $"User: {userMessage}\nAssistant: {aiResponse}";
            return updatedHistory;
        }

        private IEnumerator HandleStreamingResponse(string responseText, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            // Simplified streaming simulation - in real implementation, parse Server-Sent Events
            string fullResponse = null;
            
            // Parse response outside try-catch to avoid yield issues
            var response = JsonUtility.FromJson<OpenAIResponse>(responseText);
            fullResponse = response.choices[0].message.content;
            
            if (!string.IsNullOrEmpty(fullResponse))
            {
                // Simulate streaming by chunking the response
                string streamedContent = "";
                int chunkSize = 10;
                
                for (int i = 0; i < fullResponse.Length; i += chunkSize)
                {
                    int remainingChars = fullResponse.Length - i;
                    int currentChunkSize = UnityEngine.Mathf.Min(chunkSize, remainingChars);
                    string chunk = fullResponse.Substring(i, currentChunkSize);
                    
                    streamedContent += chunk;
                    onStreamChunk?.Invoke(streamedContent);
                    
                    yield return new WaitForSeconds(0.1f);
                }
                
                // Send final complete result
                var result = new AIResponseResult(fullResponse, "")
                {
                    isStreamComplete = true
                };
                onComplete?.Invoke(result);
            }
            else
            {
                onComplete?.Invoke(AIResponseResult.Error("Failed to parse streaming response"));
            }
        }

        private string BuildProfileSystemMessage(string profileInstruction)
        {
            return profileInstruction + "\n\nProvide a concise, updated user profile based on the conversation.";
        }

        private string BuildProfilePrompt(string newMessage, string currentProfile, string conversationHistory)
        {
            return $"Latest message: {newMessage}\n\nCurrent profile: {currentProfile}\n\nRecent conversation: {conversationHistory}\n\nPlease update the user profile.";
        }
        
        [Serializable]
        private class OpenAIResponse
        {
            public Choice[] choices;
        }
        
        [Serializable]
        private class Choice
        {
            public Message message;
        }
        
        [Serializable]
        private class Message
        {
            public string content;
        }
    }
}