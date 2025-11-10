using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Chat.Services;
using GamificationPlayer.Chat;
using System.Linq;
using UnityEngine;

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
        /// <param name="fewShotPrompt">Few-shot prompt for used to get examples via the RAG service</param>
        /// <param name="dataBankPrompt">Data bank prompt for used to get knowledge via the RAG service</param>
        /// <param name="conversationHistory">The conversation history</param>
        /// <param name="onComplete">Callback when context is retrieved</param>
        /// <returns>Coroutine IEnumerator</returns>
        public IEnumerator GetContextForUserMessage(string agentName,
            string fewShotPrompt,
            string dataBankPrompt,
            ChatManager.ChatMessage[] conversationHistory,
            Action<RAGResult> onComplete)
        {
            var lastMessage = conversationHistory.Last();

            Debug.Log($"RAGService: Getting context for agent '{agentName}' with fewShotPrompt '{fewShotPrompt}' and dataBankPrompt '{dataBankPrompt}' based on message: {lastMessage.message}");

            var examplesBasedOnMessage = GetRagForAgentAndType(agentName, RAGType.Examples)?.Ask(fewShotPrompt, 5);

            var knowledgeBasedOnMessage = GetRagForAgentAndType(agentName, RAGType.Knowledge)?.Ask(dataBankPrompt, 5);

            Debug.Log($"RAGService: Retrieved examples: {examplesBasedOnMessage}");
            Debug.Log($"RAGService: Retrieved knowledge: {knowledgeBasedOnMessage}");

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
            Debug.Log($"RAGService: Looking for RAG with agentName '{agentName}' and ragType '{ragType}'");

            var rag = rAGs.FirstOrDefault(rag => rag.AgentName == agentName && rag.RAGType == ragType);

            Debug.Log(rag != null
                ? $"RAGService: Found RAG for agentName '{agentName}' and ragType '{ragType}'"
                : $"RAGService: No RAG found for agentName '{agentName}' and ragType '{ragType}'");

            return rag;
        }
    }
}