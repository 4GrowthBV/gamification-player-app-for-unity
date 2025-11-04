using System;
using System.Collections;

namespace GamificationPlayer.Chat.Services
{
    /// <summary>
    /// Interface for AI chat service that generates responses and profiles based on instruction and context
    /// </summary>
    public interface IChatAIService
    {
        /// <summary>
        /// Generate AI response using specified instruction and conversation context with streaming support
        /// </summary>
        /// <param name="message">The user's message</param>
        /// <param name="instruction">The AI instruction (agent-specific)</param>
        /// <param name="examples">Agent-specific examples from router service</param>
        /// <param name="knowledge">Agent knowledge context from router service</param>
        /// <param name="profileContext">User profile context</param>
        /// <param name="conversationHistory">Recent conversation history for context</param>
        /// <param name="onStreamChunk">Callback for each streamed response chunk (optional for streaming)</param>
        /// <param name="onComplete">Callback with final AI response result</param>
        /// <returns>Coroutine for async operation</returns>
        IEnumerator GenerateResponse(string message, string instruction, string examples, string knowledge, string profileContext, string conversationHistory, Action<string> onStreamChunk, Action<AIResponseResult> onComplete);

        /// <summary>
        /// Generate updated user profile based on conversation context
        /// </summary>
        /// <param name="newMessage">The latest message in conversation</param>
        /// <param name="currentProfile">Current user profile</param>
        /// <param name="conversationHistory">Recent conversation history for context</param>
        /// <param name="profileInstruction">Profile generation instruction from backend</param>
        /// <param name="onComplete">Callback with profile generation result</param>
        /// <returns>Coroutine for async operation</returns>
        IEnumerator GenerateProfile(string newMessage, string currentProfile, string conversationHistory, string profileInstruction, Action<ProfileGenerationResult> onComplete);
    }

    /// <summary>
    /// Result from AI service containing the generated response
    /// </summary>
    [Serializable]
    public class AIResponseResult
    {
        public string response;
        public string conversationHistory;
        public bool success;
        public string errorMessage;
        public bool isStreamComplete;

        public AIResponseResult(string response, string conversationHistory = "")
        {
            this.response = response;
            this.conversationHistory = conversationHistory;
            this.success = !string.IsNullOrEmpty(response);
            this.errorMessage = "";
            this.isStreamComplete = true;
        }

        public static AIResponseResult Error(string errorMessage)
        {
            return new AIResponseResult("")
            {
                success = false,
                errorMessage = errorMessage,
                isStreamComplete = true
            };
        }
    }

    /// <summary>
    /// Result from profile generation containing the updated profile
    /// </summary>
    [Serializable]
    public class ProfileGenerationResult
    {
        public string updatedProfile;
        public bool success;
        public string errorMessage;

        public ProfileGenerationResult(string updatedProfile)
        {
            this.updatedProfile = updatedProfile;
            this.success = !string.IsNullOrEmpty(updatedProfile);
            this.errorMessage = "";
        }

        public static ProfileGenerationResult Error(string errorMessage)
        {
            return new ProfileGenerationResult("")
            {
                success = false,
                errorMessage = errorMessage
            };
        }
    }
}