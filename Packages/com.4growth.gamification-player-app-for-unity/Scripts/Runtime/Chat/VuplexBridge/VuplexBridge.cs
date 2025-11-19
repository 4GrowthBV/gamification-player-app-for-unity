using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Chat.DTO;
using GamificationPlayer.Chat.Services;
using Newtonsoft.Json;
using UnityEngine;
using Vuplex.WebView;

namespace GamificationPlayer.Chat
{
    public class VuplexBridge : MonoBehaviour
    {
        [SerializeField] 
        private bool enableLogging = true;
        
        [SerializeField]
        private CanvasWebViewPrefab webViewPrefab;

        [SerializeField]
        private bool forceNewConversation = false;

        private ChatManager chatManager;
        private IWebView webView;

        private IChatAIService aiService;
        private IRAGService ragService;
        private ChatManager.InitialMetadata initialMetadata;

        private bool isStreamingActive;

        public async void StartChatInitialization(ChatManager chatManager,
            IChatAIService aiService,
            ChatManager.ResumeConversationMetadata resumeMetadata, 
            ChatManager.InitialMetadata initialMetadata)
        {
            this.chatManager = chatManager;
            this.aiService = aiService;
            this.initialMetadata = initialMetadata;

            await webViewPrefab.WaitUntilInitialized();

            webView = webViewPrefab.WebView;

            StartCoroutine(VuplexBridgeMessagePuller.AutoPullVuplexMessages(webView));
            VuplexBridgeMessagePuller.OnWebViewMessage += OnWebViewMessage;
            SetupEventHandlers();

            StartCoroutine(Initialize(resumeMetadata));
        }

        private IEnumerator Initialize(ChatManager.ResumeConversationMetadata resumeMetadata)
        {            
            yield return StartCoroutine(InitializeRAGSystem());

            chatManager.InitializeChat(aiService, 
                ragService, 
                resumeMetadata, 
                initialMetadata,
                forceNewConversation);
        }

        private IEnumerator InitializeRAGSystem()
        {            
            var initTask = RAGManager.InitializeFromResourcesAsync();
            
            while (!initTask.IsCompleted)
            {
                yield return null;
            }
            
            try
            {
                if (initTask.Exception != null)
                {
                    Debug.LogError($"❌ RAG initialization failed: {initTask.Exception.GetBaseException().Message}");
                    ragService = null;
                    SendToWeb(SentToWebEventType.rag_status, new { status = "failed", error = initTask.Exception.GetBaseException().Message });
                }
                else if (initTask.Result)
                {
                    ragService = RAGManager.CreateRAGService(enableLogging);
                    SendToWeb(SentToWebEventType.rag_status, new { status = "ready", info = RAGManager.GetStatusInfo() });
                }
                else
                {
                    Debug.LogError($"❌ RAG initialization failed: {RAGManager.LastError}");
                    ragService = null;
                    SendToWeb(SentToWebEventType.rag_status, new { status = "failed", error = RAGManager.LastError });
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ RAG initialization error: {ex.Message}");
                ragService = null;
                SendToWeb(SentToWebEventType.rag_status, new { status = "error", error = ex.Message });
            }
        }

        private void SetupEventHandlers()
        {
            ChatManager.OnChatInitialized += OnChatInitialized;
            ChatManager.OnMessageReceived += OnMessageReceived;
            ChatManager.OnAIMessageReceived += OnAIMessageReceived;
            ChatManager.OnAIMessageChunkReceived += OnAIMessageChunkReceived;
            ChatManager.OnErrorOccurred += OnErrorOccurred;
        }

        private void CleanupEventHandlers()
        {
            ChatManager.OnChatInitialized -= OnChatInitialized;
            ChatManager.OnMessageReceived -= OnMessageReceived;
            ChatManager.OnAIMessageReceived -= OnAIMessageReceived;
            ChatManager.OnAIMessageChunkReceived -= OnAIMessageChunkReceived;
            ChatManager.OnErrorOccurred -= OnErrorOccurred;
        }

        private void OnDestroy()
        {
            CleanupEventHandlers();
            VuplexBridgeMessagePuller.OnWebViewMessage -= OnWebViewMessage;
        }

        private void OnChatInitialized(bool expectNewMessage)
        {            
            // Get the actual loaded conversation history
            var history = chatManager.GetConversationHistory();
            var initData = new WebInitializationData(history, expectNewMessage);

            Debug.Log("✅ Chat initialized successfully: " + JsonConvert.SerializeObject(initData));

            SendToWeb(SentToWebEventType.chat_initialized, initData);
        }

        private void OnMessageReceived(ChatManager.ChatMessage message)
        {
            // Send message to web frontend
            var webMessage = new WebChatMessage(message);
            SendToWeb(SentToWebEventType.message_received, webMessage);
        }

        private void OnAIMessageReceived(ChatManager.ChatMessage message)
        {
            // Send streaming end notification if we were streaming
            if (isStreamingActive)
            {
                isStreamingActive = false;
                SendToWeb(SentToWebEventType.ai_streaming_complete, new { timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                
                // Send final formatted message to replace streaming content
                var webMessage = new WebChatMessage(message);
                SendToWeb(SentToWebEventType.ai_message_final, webMessage);
            } else
            {
                Debug.LogError("❌ AI message received without active streaming");
            }
        }

        private void OnAIMessageChunkReceived(string chunk)
        {
            // Send streaming start event only on the first chunk
            if (!isStreamingActive)
            {
                isStreamingActive = true;
                SendToWeb(SentToWebEventType.ai_streaming_started, new { timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") });
            }

            // Send streaming chunk to web frontend
            SendToWeb(SentToWebEventType.ai_message_chunk, new { 
                chunk = chunk, 
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                isStreaming = true 
            });
        }

        private void OnErrorOccurred(string error)
        {
            // Send error to web frontend
            var errorData = new WebErrorData(error);
            SendToWeb(SentToWebEventType.error_occurred, errorData);
        }

        private void OnWebViewMessage(string message)
        {
            try
            {               
                // Parse JSON message from web
                var messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                
                if (messageData.ContainsKey("action"))
                {
                    string action = messageData["action"].ToString();
                    HandleWebAction(action, messageData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"VuplexBridge OnWebViewMessage Error: {ex.Message}");

                SendToWeb(SentToWebEventType.error_occurred, new WebErrorData($"Error handling message: {ex.Message}"));
            }
        }

        private void HandleWebAction(string action, Dictionary<string, object> data)
        {
            switch (action.ToLower())
            {
                case nameof(ReceivedFromWebEventType.send_message):
                    if (data.ContainsKey("message"))
                    {
                        string message = data["message"].ToString();
                        SendUserMessage(message);
                    }
                    break;

                case nameof(ReceivedFromWebEventType.click_button):
                    if (data.ContainsKey("buttonId"))
                    {
                        string buttonId = data["buttonId"].ToString();
                        HandleButtonClick(buttonId);
                    }
                    break;

                case nameof(ReceivedFromWebEventType.user_activity):
                    if (data.ContainsKey("activityData"))
                    {
                        var activityJson = data["activityData"].ToString();
                        var activityData = JsonConvert.DeserializeObject<Dictionary<string, string>>(activityJson);
                        HandleUserActivity(activityData);
                    }
                    break;

                case nameof(ReceivedFromWebEventType.force_new_conversation):
                    ForceNewConversation();
                    break;

                case nameof(ReceivedFromWebEventType.get_conversation_history):
                    SendConversationHistory();
                    break;

                default:
                    Debug.LogWarning($"VuplexBridge Unknown action: {action}");
                    SendToWeb(SentToWebEventType.error_occurred, new WebErrorData($"Unknown action: {action}"));
                    break;
            }
        }

        private void SendUserMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || aiService == null || ragService == null)
            {
                Debug.LogError("❌ Cannot send message: Missing services or empty message");
                SendToWeb(SentToWebEventType.error_occurred, new WebErrorData("Cannot send message: Missing services or empty message"));
                return;
            }
                        
            chatManager.HandleUserMessage(aiService, ragService, message);
        }

        private void HandleButtonClick(string buttonId)
        {
            if (aiService == null)
            {
                Debug.LogError("❌ Cannot handle button click: Missing AI service");
                SendToWeb(SentToWebEventType.error_occurred, new WebErrorData("Cannot handle button click: Missing AI service"));
                return;
            }
            
            chatManager.HandleButtonClick(aiService, buttonId);
        }
        
        private void HandleUserActivity(Dictionary<string, string> userActivityMetadata)
        {
            if (aiService == null)
            {
                Debug.LogError("❌ Cannot handle user activity: Missing AI service");
                SendToWeb(SentToWebEventType.error_occurred, new WebErrorData("Cannot handle user activity: Missing AI service"));
                return;
            }
            
            chatManager.HandleUserActivity(aiService, ragService, userActivityMetadata);
        }

        private void ForceNewConversation()
        {           
            chatManager.ForceNewConversation(aiService, ragService, initialMetadata);
        }

        private void SendConversationHistory()
        {
            var history = chatManager.GetConversationHistory();
            var webHistory = new WebChatMessage[history.Count];
            
            for (int i = 0; i < history.Count; i++)
            {
                webHistory[i] = new WebChatMessage(history[i]);
            }
            
            SendToWeb(SentToWebEventType.conversation_history, new { history = webHistory });
        }

        private async void SendToWeb(SentToWebEventType eventType, object data)
        {
            if (webView == null) return;

            try
            {
                var webEventData = new WebEventData(eventType, data);
                string json = JsonConvert.SerializeObject(webEventData);
                
                // Send to JavaScript via postMessage
                string script = $"window.postMessage({json}, '*');";
                var result = await webView.ExecuteJavaScript(script);
            }
            catch (Exception ex)
            {
                Debug.LogError($"VuplexBridge SendToWeb Error: {ex.Message}");
            }
        }
    }
}
