using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Chat.Services;
using GamificationPlayer.Chat;
using System.Linq;

namespace GamificationPlayer
{
    /// <summary>
    /// RAG service to fetch examples & knowledge context
    /// </summary>
    public class RAGService : IRAGService
    {
        // RAG instances for fetching examples & knowledge per agent
        private readonly List<IRAG> rAGs;

        /// <summary>
        /// Constructor for RAGService
        /// </summary>
        /// <param name="ragExamples">RAG instances for fetching examples per agent</param>
        /// <param name="ragKnowledge">RAG instances for fetching knowledge per agent</param>
        public RAGService(List<IRAG> rAGs)
        {
            this.rAGs = rAGs;
        }

        /// <summary>
        /// Get context (examples and knowledge) for user message based on the chosen agent's name and conversation history
        /// </summary>
        /// <param name="agentName">The name of the agent</param>
        /// <param name="conversationHistory">The conversation history</param>
        /// <param name="onComplete">Callback when context is retrieved</param>
        /// <returns>Coroutine IEnumerator</returns>
        public IEnumerator GetContextForUserMessage(string agentName,
            ChatManager.ChatMessage[] conversationHistory,
            Action<RAGResult> onComplete)
        {
            var lastMessage = conversationHistory.Last();

            var examplesBasedOnMessage = GetRagForAgentAndType(agentName, RAGType.Examples)?.Ask(lastMessage.message, 5);

            var knowledgeBasedOnMessage = GetRagForAgentAndType(agentName, RAGType.Knowledge)?.Ask(lastMessage.message, 5);

            onComplete?.Invoke(new RAGResult(examplesBasedOnMessage, knowledgeBasedOnMessage));

            yield return null;
        }

        /// <summary>
        /// Get RAG instance for the given agent name and RAG type
        /// </summary>
        /// <param name="agentName"></param>
        /// <param name="ragType"></param>
        private IRAG GetRagForAgentAndType(string agentName, RAGType ragType)
        {
            return rAGs.FirstOrDefault(rag => rag.AgentName == agentName && rag.RAGType == ragType);
        }
    }
}