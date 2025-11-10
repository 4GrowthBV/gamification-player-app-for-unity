using System;
using System.Collections;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class RAGMockService : IRAGService
    {       
        public IEnumerator GetContextForUserMessage(string agentName,
            string fewShotPrompt,
            string dataBankPrompt,
            ChatManager.ChatMessage[] conversationHistory,
            Action<RAGResult> onComplete)
        {
            // Fast mock response for testing purposes (reduced delay for performance tests)
            yield return new WaitForSeconds(0.1f); // Minimal delay for testing

            string mockExamples = "Example 1: ...\nExample 2: ...";
            string mockKnowledge = "This is some mock knowledge context.";

            var result = new RAGResult(mockExamples, mockKnowledge);
            onComplete?.Invoke(result); 
        }
    }
}