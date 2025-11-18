using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using GamificationPlayer.DTO.ExternalEvents;
using Vuplex.WebView;
using Newtonsoft.Json;

namespace GamificationPlayer.TestBed
{
    /// <summary>
    /// ChatManager Vuplex Test Bed - Bridges ChatManager with HTML/JavaScript frontend via Vuplex WebView
    /// This test bed exposes ChatManager functionality to web-based UIs through JavaScript events
    /// </summary>
    public class ChatManagerVuplexTestBed : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private EnvironmentConfig environmentConfig;
        
        private string htmlPageUrl = "streaming-assets://chat-interface.html";
        
        [Header("Vuplex WebView")]
        [SerializeField] private CanvasWebViewPrefab webViewPrefab;

        #region Core Components
        private ChatManager chatManager;
        private GamificationPlayerEndpoints endpoints;
        private ISessionLogData sessionData;
        private IWebView webView;
        #endregion

        #region Services
        private IChatAIService aiService;
        private IRAGService ragService;
        #endregion

        #region Chat State
        private bool isInitialized = false;
        private bool isConnected = false;
        private string connectionStatus = "Disconnected";
        private bool isStreamingActive = false;
        #endregion

        #region Event Monitoring
        private List<string> eventLog = new List<string>();
        #endregion

        #region Performance Metrics
        private float lastResponseTime = 0f;
        private DateTime lastOperationStart;
        #endregion

        #region Serializable Data Classes for JSON

        [Serializable]
        public class WebChatMessage
        {
            public string role;
            public string message;
            public WebButton[] buttons;
            public string timestamp;
            public string buttonName;
            public Dictionary<string, string> userActivityMetadata;

            public WebChatMessage(ChatManager.ChatMessage chatMessage)
            {
                role = chatMessage.role;
                message = chatMessage.message;
                timestamp = chatMessage.timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                buttonName = chatMessage.buttonName;
                userActivityMetadata = chatMessage.userActivityMetadata;

                if (chatMessage.buttons != null)
                {
                    buttons = new WebButton[chatMessage.buttons.Length];
                    for (int i = 0; i < chatMessage.buttons.Length; i++)
                    {
                        buttons[i] = new WebButton(chatMessage.buttons[i]);
                    }
                }
            }
        }

        [Serializable]
        public class WebButton
        {
            public string identifier;
            public string text;

            public WebButton(ChatManager.Button button)
            {
                identifier = button.identifier;
                text = button.text;
            }
        }

        [Serializable]
        public class WebEventData
        {
            public string eventType;
            public object data;
            public string timestamp;

            public WebEventData(string eventType, object data)
            {
                this.eventType = eventType;
                this.data = data;
                this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }

        [Serializable]
        public class WebErrorData
        {
            public string error;
            public string timestamp;

            public WebErrorData(string error)
            {
                this.error = error;
                this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }

        [Serializable]
        public class WebInitializationData
        {
            public string status;
            public string connectionStatus;
            public WebChatMessage[] conversationHistory;
            public string timestamp;
            public bool expectNewMessage;

            public WebInitializationData(string status, string connectionStatus, List<ChatManager.ChatMessage> history, bool expectNewMessage = false)
            {
                this.status = status;
                this.connectionStatus = connectionStatus;
                this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                this.expectNewMessage = expectNewMessage;

                if (history != null)
                {
                    conversationHistory = new WebChatMessage[history.Count];
                    for (int i = 0; i < history.Count; i++)
                    {
                        conversationHistory[i] = new WebChatMessage(history[i]);
                    }
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        async void Awake()
        {
            LogEvent("=== ChatManager Vuplex Test Bed Starting ===");

            await webViewPrefab.WaitUntilInitialized();

            webView = webViewPrefab.WebView;
            
            // Load the HTML page
            LogEvent($"Loading HTML page: {htmlPageUrl}");
            webView.LoadUrl(htmlPageUrl);
            
            await webView.WaitForNextPageLoadToFinish();
            
            Debug.Log("ChatManager Vuplex Test Bed started.");
        }

        async void Start()
        {
            // Initialize ChatManager components
            InitializeTestBed();

            Debug.Log("ChatManager components initialized.");
            
            if (autoInitialize)
            {
                StartCoroutine(AutoInitialize());
            }

            StartCoroutine(AutoPullVuplexMessages());
        }

        private IEnumerator AutoPullVuplexMessages()
        {
            // Wait until webView is ready (important for Vuplex)
            while (webView == null || !webView.IsInitialized)
            {
                yield return new WaitForSeconds(0.5f);
            }

            // Optional: wait one frame after init
            yield return null;

            while (true)
            {
                yield return new WaitForSeconds(0.5f); // Poll every 500ms

                if (webView == null) continue;

                // Use Vuplex's coroutine-friendly ExecuteJavaScript with callback
                webView.ExecuteJavaScript(GetPullScript(), OnJavaScriptResult);
            }
        }

        private string GetPullScript()
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

        private void OnJavaScriptResult(string result)
        {
            if (string.IsNullOrEmpty(result) || result == "[]" || result == "null")
                return;

            // This callback runs on the main thread — safe to process
            LogEvent($"Pulled messages: {result}");

            try
            {
                var messages = JsonConvert.DeserializeObject<List<string>>(result);
                foreach (var message in messages)
                {
                    // Use Unity's main thread dispatcher if needed
                    OnWebViewMessage(this, new EventArgs<string>(message));
                }
            }
            catch (System.Exception ex)
            {
                LogEvent($"Failed to parse messages: {ex.Message}\nResult was: {result}");
            }
        }

        void OnDestroy()
        {
            CleanupEventHandlers();
            
            if (webView != null)
            {
                webView.MessageEmitted -= OnWebViewMessage;
            }
        }

        #endregion

        #region ChatManager Initialization

        private void InitializeTestBed()
        {
            try
            {               
                // Setup services
                SetupServiceOptions();
                
                // Initialize core components
                InitializeCoreComponents();
                
                // Setup event handlers
                SetupEventHandlers();
                
                LogEvent("Test Bed initialized successfully");
                connectionStatus = "Ready";
                
                // Notify web frontend
                SendToWeb("testbed_initialized", new { status = "ready" });
                
            }
            catch (Exception ex)
            {
                LogEvent($"Failed to initialize test bed: {ex.Message}");
                connectionStatus = "Error";
                SendToWeb("testbed_error", new WebErrorData(ex.Message));
            }
        }

        private void SetupServiceOptions()
        {
            aiService = new ChatAIService("", isLoggingEnabled: enableLogging);
            ragService = null; // Will be initialized asynchronously
        }

        private void InitializeCoreComponents()
        {
            // Create mock session data
            sessionData = new SessionLogData();

            sessionData.AddToLog(new MicroGamePayload
            {
                player = new MicroGamePayload.Player
                {
                    user_id = "5b411dd2-20c1-49dd-90a5-555dbaead5f8",
                },
                organisation = new MicroGamePayload.Organisation
                {
                    id = "edb5e165-1c74-44f8-8d57-c24b82f2f5f2",
                },
                micro_game = new MicroGamePayload.MicroGame
                {
                    id = "99d75cfb-ce23-4939-a755-013d04a435c8",
                }
            });
            
            // Create Gamification Player endpoints
            if (environmentConfig != null)
            {
                endpoints = new GamificationPlayerEndpoints(environmentConfig, sessionData);
            }
            
            // Create ChatManager
            chatManager = gameObject.AddComponent<ChatManager>();
            if (endpoints != null && sessionData != null)
            {
                chatManager.IsLogging = enableLogging;
                chatManager.Initialize(endpoints, sessionData);
                isInitialized = chatManager.IsInitialized();
            }
        }

        private IEnumerator AutoInitialize()
        {
            yield return new WaitForSeconds(0.5f); // Let everything settle
            
            if (isInitialized)
            {
                LogEvent("Auto-initializing RAG system...");
                yield return StartCoroutine(InitializeRAGSystem());
                
                LogEvent("Auto-initializing chat...");
                StartChatInitialization();
            }
        }

        private IEnumerator InitializeRAGSystem()
        {
            LogEvent("Starting RAG system initialization...");
            
            var initTask = RAGManager.InitializeFromResourcesAsync();
            
            while (!initTask.IsCompleted)
            {
                yield return null;
            }
            
            try
            {
                if (initTask.Exception != null)
                {
                    LogEvent($"❌ RAG initialization failed: {initTask.Exception.GetBaseException().Message}");
                    ragService = null;
                    SendToWeb("rag_status", new { status = "failed", error = initTask.Exception.GetBaseException().Message });
                }
                else if (initTask.Result)
                {
                    LogEvent("✅ RAG system initialized successfully");
                    ragService = RAGManager.CreateRAGService(enableLogging);
                    LogEvent($"RAG Status: {RAGManager.GetStatusInfo()}");
                    SendToWeb("rag_status", new { status = "ready", info = RAGManager.GetStatusInfo() });
                }
                else
                {
                    LogEvent($"❌ RAG initialization failed: {RAGManager.LastError}");
                    ragService = null;
                    SendToWeb("rag_status", new { status = "failed", error = RAGManager.LastError });
                }
            }
            catch (System.Exception ex)
            {
                LogEvent($"❌ RAG initialization error: {ex.Message}");
                ragService = null;
                SendToWeb("rag_status", new { status = "error", error = ex.Message });
            }
        }

        #endregion

        #region ChatManager Event Handlers

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

        private void OnChatInitialized(bool expectNewMessage)
        {
            LogEvent($"✓ OnChatInitialized - History loaded, expectNewMessage: {expectNewMessage}");
            RecordPerformanceMetric("Chat Initialization");

            // Now that history is loaded, set connected status and send it
            isConnected = true;
            connectionStatus = "Connected";
            
            // Get the actual loaded conversation history
            var history = chatManager.GetConversationHistory();
            var initData = new WebInitializationData("initialized", connectionStatus, history, expectNewMessage);
            SendToWeb("chat_initialized", initData);
        }

        private void OnMessageReceived(ChatManager.ChatMessage message)
        {
            LogEvent($"✓ OnMessageReceived: \"{message.message.Substring(0, Math.Min(50, message.message.Length))}...\"");
            RecordPerformanceMetric("Message Received");

            // Send message to web frontend
            var webMessage = new WebChatMessage(message);
            SendToWeb("message_received", webMessage);
        }

        private void OnAIMessageReceived(ChatManager.ChatMessage message)
        {
            LogEvent($"✓ OnAIMessageReceived: \"{message.message.Substring(0, Math.Min(50, message.message.Length))}...\"");
            RecordPerformanceMetric("AI Response Received");

            // Send streaming end notification if we were streaming
            if (isStreamingActive)
            {
                isStreamingActive = false;
                SendToWeb("ai_streaming_complete", new { timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                
                // Send final formatted message to replace streaming content
                var webMessage = new WebChatMessage(message);
                SendToWeb("ai_message_final", webMessage);
            }
            else
            {
                // No streaming occurred, send as regular AI message
                var webMessage = new WebChatMessage(message);
                SendToWeb("ai_message_received", webMessage);
            }
        }

        private void OnAIMessageChunkReceived(string chunk)
        {
            LogEvent($"⚡ Stream chunk: \"{chunk.Substring(0, Math.Min(30, chunk.Length))}...\"");

            // Send streaming start event only on the first chunk
            if (!isStreamingActive)
            {
                isStreamingActive = true;
                SendToWeb("ai_streaming_started", new { timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") });
            }

            // Send streaming chunk to web frontend
            SendToWeb("ai_message_chunk", new { 
                chunk = chunk, 
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                isStreaming = true 
            });
        }

        private void OnErrorOccurred(string error)
        {
            LogEvent($"❌ OnErrorOccurred: {error}");
            connectionStatus = $"Error: {error}";

            // Send error to web frontend
            var errorData = new WebErrorData(error);
            SendToWeb("error_occurred", errorData);
        }

        #endregion

        #region WebView Communication

        private void OnWebViewMessage(object sender, EventArgs<string> eventArgs)
        {
            try
            {
                LogEvent($"📨 Message from web: {eventArgs.Value}");
                
                // Parse JSON message from web
                var messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventArgs.Value);
                
                if (messageData.ContainsKey("action"))
                {
                    string action = messageData["action"].ToString();
                    HandleWebAction(action, messageData);
                }
            }
            catch (Exception ex)
            {
                LogEvent($"❌ Error handling web message: {ex.Message}");
                SendToWeb("error", new WebErrorData($"Error handling message: {ex.Message}"));
            }
        }

        private void HandleWebAction(string action, Dictionary<string, object> data)
        {
            switch (action.ToLower())
            {
                case "initialize_chat":
                    StartChatInitialization();
                    break;

                case "send_message":
                    if (data.ContainsKey("message"))
                    {
                        string message = data["message"].ToString();
                        SendUserMessage(message);
                    }
                    break;

                case "click_button":
                    if (data.ContainsKey("buttonId"))
                    {
                        string buttonId = data["buttonId"].ToString();
                        HandleButtonClick(buttonId);
                    }
                    break;

                case "user_activity":
                    if (data.ContainsKey("activityData"))
                    {
                        var activityJson = data["activityData"].ToString();
                        var activityData = JsonConvert.DeserializeObject<Dictionary<string, string>>(activityJson);
                        HandleUserActivity(activityData);
                    }
                    break;

                case "force_new_conversation":
                    ForceNewConversation();
                    break;

                case "get_conversation_history":
                    SendConversationHistory();
                    break;

                case "get_status":
                    SendStatus();
                    break;

                default:
                    LogEvent($"❓ Unknown web action: {action}");
                    SendToWeb("error", new WebErrorData($"Unknown action: {action}"));
                    break;
            }
        }

        private void SendToWeb(string eventType, object data)
        {
            if (webView == null) return;

            try
            {
                var webEventData = new WebEventData(eventType, data);
                string json = JsonConvert.SerializeObject(webEventData);
                
                // Send to JavaScript via postMessage
                string script = $"window.postMessage({json}, '*');";
                webView.ExecuteJavaScript(script);
                
                LogEvent($"📤 Sent to web: {eventType}");
            }
            catch (Exception ex)
            {
                LogEvent($"❌ Error sending to web: {ex.Message}");
            }
        }

        #endregion

        #region Chat Operations

        private void StartChatInitialization()
        {
            if (chatManager == null || aiService == null)
            {
                LogEvent("❌ Cannot initialize: Missing ChatManager or AI service");
                SendToWeb("error", new WebErrorData("Cannot initialize: Missing ChatManager or AI service"));
                return;
            }
            
            LogEvent("Starting chat initialization...");
            StartPerformanceTimer();

            var resumeMetadata = new ChatManager.ResumeConversationMetadata();
            var initialMetadata = new ChatManager.InitialMetadata("Wellness Buddy", "WebUser", DateTime.Now);
            
            chatManager.InitializeChat(aiService, ragService, resumeMetadata, initialMetadata);
        }

        private void SendUserMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || aiService == null || ragService == null)
            {
                LogEvent("❌ Cannot send message: Missing services or empty message");
                SendToWeb("error", new WebErrorData("Cannot send message: Missing services or empty message"));
                return;
            }
            
            LogEvent($"📤 Sending user message: {message}");
            StartPerformanceTimer();
            
            chatManager.HandleUserMessage(aiService, ragService, message);
        }

        private void HandleButtonClick(string buttonId)
        {
            if (aiService == null)
            {
                LogEvent("❌ Cannot handle button click: Missing AI service");
                SendToWeb("error", new WebErrorData("Cannot handle button click: Missing AI service"));
                return;
            }

            LogEvent($"🔘 Button clicked: {buttonId}");
            StartPerformanceTimer();
            
            chatManager.HandleButtonClick(aiService, buttonId);
        }
        
        private void HandleUserActivity(Dictionary<string, string> userActivityMetadata)
        {
            if (aiService == null)
            {
                LogEvent("❌ Cannot handle user activity: Missing AI service");
                SendToWeb("error", new WebErrorData("Cannot handle user activity: Missing AI service"));
                return;
            }

            LogEvent($"🔘 User activity detected: {JsonConvert.SerializeObject(userActivityMetadata)}");
            StartPerformanceTimer();
            
            chatManager.HandleUserActivity(aiService, ragService, userActivityMetadata);
        }

        private void ForceNewConversation()
        {
            LogEvent("Force new conversation requested");
            
            var initialMetadata = new ChatManager.InitialMetadata("Fresh Assistant", "WebUser", DateTime.Now);
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
            
            SendToWeb("conversation_history", new { history = webHistory });
        }

        private void SendStatus()
        {
            var status = new
            {
                isInitialized = isInitialized,
                isConnected = isConnected,
                connectionStatus = connectionStatus,
                lastResponseTime = lastResponseTime,
                ragStatus = ragService != null ? "ready" : "not_available"
            };
            
            SendToWeb("status_update", status);
        }

        #endregion

        #region Performance Tracking

        private void StartPerformanceTimer()
        {
            lastOperationStart = DateTime.Now;
        }

        private void RecordPerformanceMetric(string operation)
        {
            lastResponseTime = (float)(DateTime.Now - lastOperationStart).TotalSeconds;
            LogEvent($"⏱ {operation} completed in {lastResponseTime:F2}s");
            
            // Send performance data to web
            SendToWeb("performance_metric", new 
            { 
                operation = operation, 
                responseTime = lastResponseTime,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            });
        }

        #endregion

        #region Utility Methods

        private void LogEvent(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {message}";
            
            eventLog.Add(logEntry);
            
            // Keep log size manageable
            if (eventLog.Count > 100)
            {
                eventLog.RemoveAt(0);
            }
            
            if (enableLogging)
            {
                Debug.Log($"[VuplexTestBed] {message}");
            }
        }

        #endregion
    }
}