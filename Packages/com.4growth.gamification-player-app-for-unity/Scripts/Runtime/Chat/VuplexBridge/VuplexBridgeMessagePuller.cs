using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Vuplex.WebView;

namespace GamificationPlayer.Chat
{
    public class VuplexBridgeMessagePuller
    {
        public static event Action<string> OnWebViewMessage;

        public static IEnumerator AutoPullVuplexMessages(IWebView webView)
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f); // Poll every 500ms

                if (webView == null) continue;

                // Use Vuplex's coroutine-friendly ExecuteJavaScript with callback
                webView.ExecuteJavaScript(GetPullScript(), OnJavaScriptResult);
            }
        }

        private static string GetPullScript()
        {
            return @"
                (function() {
                    try {
                        var messages = window._pullMessages || [];
                        var result = messages.slice(); // copy
                        window._pullMessages = [];   // clear
                        return JSON.stringify(result);
                    } catch (e) {
                        console.error('Vuplex pull error:', e);
                        return JSON.stringify([]);
                    }
                })();
            ";
        }

        private static void OnJavaScriptResult(string result)
        {
            if (string.IsNullOrEmpty(result) || result == "[]" || result == "null")
                return;

            try
            {
                var messages = JsonConvert.DeserializeObject<List<string>>(result);
                foreach (var message in messages)
                {
                    // Use Unity's main thread dispatcher if needed
                    OnWebViewMessage?.Invoke(message);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"VuplexBridgeMessagePuller: Failed to parse messages: {ex.Message}");
            }
        }
    }
}
