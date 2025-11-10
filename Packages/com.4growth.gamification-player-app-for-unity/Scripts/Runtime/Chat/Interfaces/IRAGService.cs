using System;
using System.Collections;
using System.Collections.Generic;

namespace GamificationPlayer.Chat.Services
{
    /// <summary>
    /// Interface for chat RAG service to get examples and knowledge context for user messages
    /// </summary>
    public interface IRAGService
    {
        /// <summary>
        /// Get context (examples and knowledge) for user message based on the chosen agent's name and conversation history
        /// </summary>
        /// <param name="agentName">The name of the AI agent</param>
        /// <param name="fewShotPrompt">Few-shot prompt for used to get examples via the RAG service</param>
        /// <param name="dataBankPrompt">Data bank prompt for used to get knowledge via the RAG service</param>
        /// <param name="conversationHistory">Recent conversation history for context</param>
        /// <param name="onComplete">Callback with router result containing examples and knowledge</param>
        IEnumerator GetContextForUserMessage(string agentName,
            string fewShotPrompt,
            string dataBankPrompt,
            ChatManager.ChatMessage[] conversationHistory,
            Action<RAGResult> onComplete);
    }

    /// <summary>
    /// Interface for RAG (Retrieval-Augmented Generation) instances
    /// </summary>
    public interface IRAG
    {
        public RAGType RAGType { get; } // Examples or Knowledge
        public string AgentName { get; } // Agent name this RAG is associated with
        List<RagHit> Search(string query, int topK = 5); // Retrieve top-K relevant chunks for the query
        string Ask(string query, int topK = 5, int maxChars = 1200); // Get stitched context for the query
        void Dispose(); // Dispose resources if needed
    }

    /// <summary>
    /// Types of RAG context
    /// </summary>
    public enum RAGType
    {
        Examples,
        Knowledge
    }

    /// <summary>
    /// Result from RAG service containing examples and knowledge context
    /// </summary>
    [Serializable]
    public class RAGResult
    {
        public string examples; //  Examples to provide context to the agent
        public string knowledge; // Knowledge context for the agent
        public bool success;
        public string errorMessage;

        public RAGResult(string examples, string knowledge)
        {
            this.examples = examples;
            this.knowledge = knowledge;
            this.success = !string.IsNullOrEmpty(examples) || !string.IsNullOrEmpty(knowledge);
            this.errorMessage = "";
        }

        public static RAGResult Error(string errorMessage)
        {
            return new RAGResult("", "")
            {
                success = false,
                errorMessage = errorMessage
            };
        }
    }
}