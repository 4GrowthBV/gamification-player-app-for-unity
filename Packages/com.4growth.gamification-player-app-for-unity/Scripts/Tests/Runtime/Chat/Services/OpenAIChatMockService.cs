using System;
using System.Collections;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    /// <summary>
    /// Example implementation of IChatAIService using OpenAI API
    /// This would typically be in a separate package/assembly
    /// </summary>
    public class OpenAIChatMockService : IChatAIService
    {
       
        /// <summary>
        /// Generate AI response with streaming support (mock implementation)
        /// </summary>
        public IEnumerator GenerateResponse(string instruction, string examples, string knowledge, string profileContext, ChatManager.ChatMessage[] conversationHistory, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            // Fast mock response for testing purposes (reduced delay for performance tests)
            yield return new WaitForSeconds(0.1f); // Minimal delay for testing

            string mockResponse = $"[Mocked Response using instruction '{instruction}']";
            
            // Simulate fast streaming if callback provided (optimized for performance tests)
            if (onStreamChunk != null)
            {
                string streamedContent = "";
                // Use larger chunks and shorter delays for faster testing
                for (int i = 0; i < mockResponse.Length; i += 20) // Bigger chunks = fewer iterations
                {
                    int chunkLength = Mathf.Min(20, mockResponse.Length - i);
                    streamedContent += mockResponse.Substring(i, chunkLength);
                    onStreamChunk?.Invoke(streamedContent);
                    yield return new WaitForSeconds(0.02f); // Much faster streaming delay
                }
            }

            var result = new AIResponseResult(mockResponse);
            onComplete?.Invoke(result); 
        }

        /// <summary>
        /// Generate updated user profile (mock implementation)
        /// </summary>
        public IEnumerator GenerateProfile(string currentProfile, ChatManager.ChatMessage[] conversationHistory, string profileInstruction, Action<AIResponseResult> onComplete)
        {
            // Fast mock profile generation for testing
            yield return new WaitForSeconds(0.05f); // Minimal delay for testing

            string mockUpdatedProfile = $"Updated profile based on message\nPrevious profile: {currentProfile}\nTimestamp: {System.DateTime.Now}";

            var result = new AIResponseResult(mockUpdatedProfile);
            onComplete?.Invoke(result);
        }

        public IEnumerator GetAIAgentName(ChatManager.ChatMessage[] conversationHistory,
            string getAIAgentNameInstruction,
            Action<AIResponseResult> onComplete)
        {
            // Fast mock agent name retrieval for testing
            yield return new WaitForSeconds(0.05f); // Minimal delay for testing

            string mockAgentName = "MockedAgent";

            var result = new AIResponseResult(mockAgentName);
            onComplete?.Invoke(result);
        }
    }
}