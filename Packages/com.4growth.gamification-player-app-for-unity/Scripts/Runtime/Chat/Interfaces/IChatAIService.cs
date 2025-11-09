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
        /// <param name="instruction">The AI instruction (agent-specific)</param>
        /// <param name="examples">Agent-specific examples from router service</param>
        /// <param name="knowledge">Agent knowledge context from router service</param>
        /// <param name="profileContext">User profile context</param>
        /// <param name="conversationHistory">Recent conversation history for context with the latest message in conversation</param>
        /// <param name="onStreamChunk">Callback for each streamed response chunk (optional for streaming)</param>
        /// <param name="onComplete">Callback with final AI response result</param>
        /// <returns>Coroutine for async operation</returns>
        IEnumerator GenerateResponse(string instruction,
            string examples,
            string knowledge,
            string profileContext,
            ChatManager.ChatMessage[] conversationHistory,
            Action<string> onStreamChunk,
            Action<AIResponseResult> onComplete);

        /// <summary>
        /// Generate updated user profile based on conversation context
        /// </summary>
        /// <param name="currentProfile">Current user profile</param>
        /// <param name="conversationHistory">Recent conversation history for context with the latest message in conversation</param>
        /// <param name="profileInstruction">Profile generation instruction from backend</param>
        /// <param name="onComplete">Callback with profile generation result</param>
        /// <returns>Coroutine for async operation</returns>
        IEnumerator GenerateProfile(string currentProfile,
            ChatManager.ChatMessage[] conversationHistory,
            string profileInstruction,
            Action<AIResponseResult> onComplete);

        /// <summary>
        /// Get AI agent's name based on conversation history
        /// </summary>
        /// <param name="conversationHistory">Recent conversation history for context</param>
        /// <param name="getAIAgentNameInstruction">Instruction to get AI agent's name</param>
        /// <param name="onComplete">Callback with AI agent name result</param>
        /// <returns>Coroutine for async operation</returns>
        IEnumerator GetAIAgentName(ChatManager.ChatMessage[] conversationHistory,
            string getAIAgentNameInstruction,
            Action<AIResponseResult> onComplete);
    }

    /// <summary>
    /// Result from AI service containing the generated response
    /// </summary>
    [Serializable]
    public class AIResponseResult
    {
        public string response;
        public bool success;
        public string errorMessage;

        public AIResponseResult(string response)
        {
            this.response = response;
            this.success = !string.IsNullOrEmpty(response);
            this.errorMessage = "";
        }

        public static AIResponseResult Error(string errorMessage)
        {
            return new AIResponseResult("")
            {
                success = false,
                errorMessage = errorMessage
            };
        }
    }
}