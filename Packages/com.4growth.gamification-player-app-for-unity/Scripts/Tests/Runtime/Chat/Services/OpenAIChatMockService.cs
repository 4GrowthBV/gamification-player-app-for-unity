using System;
using System.Collections;
using GamificationPlayer.Chat.Services;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    /// <summary>
    /// Example implementation of IChatAIService using OpenAI API
    /// This would typically be in a separate package/assembly
    /// </summary>
    public class OpenAIChatMockService : MonoBehaviour, IChatAIService
    {
       
        /// <summary>
        /// Generate AI response with streaming support (mock implementation)
        /// </summary>
        public IEnumerator GenerateResponse(string message, string instruction, string examples, string knowledge, string profileContext, string conversationHistory, Action<string> onStreamChunk, Action<AIResponseResult> onComplete)
        {
            // Fast mock response for testing purposes (reduced delay for performance tests)
            yield return new WaitForSeconds(0.1f); // Minimal delay for testing

            string mockResponse = $"[Mocked Response using instruction '{instruction}'] to message: {message}";
            
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

            string updatedHistory = $"{conversationHistory}\nUser: {message}\nAssistant: {mockResponse}";
            var result = new AIResponseResult(mockResponse, updatedHistory)
            {
                isStreamComplete = true
            };
            onComplete?.Invoke(result); 
        }

        /// <summary>
        /// Generate updated user profile (mock implementation)
        /// </summary>
        public IEnumerator GenerateProfile(string newMessage, string currentProfile, string conversationHistory, string profileInstruction, Action<ProfileGenerationResult> onComplete)
        {
            // Fast mock profile generation for testing
            yield return new WaitForSeconds(0.05f); // Minimal delay for testing

            string mockUpdatedProfile = $"Updated profile based on message: {newMessage}\nPrevious profile: {currentProfile}\nTimestamp: {System.DateTime.Now}";
            
            var result = new ProfileGenerationResult(mockUpdatedProfile);
            onComplete?.Invoke(result);
        }
    }
}