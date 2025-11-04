using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using GamificationPlayer.Chat.Services;

namespace GamificationPlayer.TestBed.ProductionServices
{
    /// <summary>
    /// Test N8n Router Service integration for ChatManager test bed
    /// Implements test N8n API calls to your staging environment
    /// </summary>
    public class N8nRouterServiceTest : MonoBehaviour, IChatRouterService
    {
        /// <summary>
        /// Route message through N8n workflow to determine appropriate agent
        /// </summary>
        public IEnumerator RouteMessage(string userMessage, string conversationHistory, Action<RouterResult> onComplete)
        {
            // test

            yield return new WaitForSeconds(0.1f); // Simulate network delay

            onComplete?.Invoke(new RouterResult("n8n_agent", "Example 1: ...\nExample 2: ...", "This is some knowledge context."));
        }
    }
}