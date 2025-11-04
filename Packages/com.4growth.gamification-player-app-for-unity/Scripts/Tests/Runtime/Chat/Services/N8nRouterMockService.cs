using System;
using System.Collections;
using GamificationPlayer.Chat.Services;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class N8nRouterMockService : MonoBehaviour, IChatRouterService
    {       
        public IEnumerator RouteMessage(string userMessage, string conversationHistory, Action<RouterResult> onComplete)
        {
            // Fast mock response for testing purposes (reduced delay for performance tests)
            yield return new WaitForSeconds(0.1f); // Minimal delay for testing

            string mockAgent = "mock_agent";
            string mockExamples = "Example 1: ...\nExample 2: ...";
            string mockKnowledge = "This is some mock knowledge context.";

            var result = new RouterResult(mockAgent, mockExamples, mockKnowledge);
            onComplete?.Invoke(result); 
        }
    }
}