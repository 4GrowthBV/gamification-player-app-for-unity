using System;
using System.Collections;

namespace GamificationPlayer.Chat.Services
{
    /// <summary>
    /// Interface for chat routing service that determines which agent should handle a user message
    /// </summary>
    public interface IChatRouterService
    {
        /// <summary>
        /// Analyze user message and conversation history to determine appropriate agent
        /// </summary>
        /// <param name="userMessage">The user's message</param>
        /// <param name="conversationHistory">Recent conversation context</param>
        /// <param name="onComplete">Callback with routing result</param>
        /// <returns>Coroutine for async operation</returns>
        IEnumerator RouteMessage(string userMessage, string conversationHistory, Action<RouterResult> onComplete);
    }

    /// <summary>
    /// Result from router service containing agent selection and context
    /// </summary>
    [Serializable]
    public class RouterResult
    {
        public string agent;
        public string examples;
        public string knowledge;
        public bool success;
        public string errorMessage;

        public RouterResult(string agent, string examples = "", string knowledge = "")
        {
            this.agent = agent;
            this.examples = examples;
            this.knowledge = knowledge;
            this.success = !string.IsNullOrEmpty(agent);
            this.errorMessage = "";
        }

        public static RouterResult Error(string errorMessage)
        {
            return new RouterResult("")
            {
                success = false,
                errorMessage = errorMessage
            };
        }
    }
}