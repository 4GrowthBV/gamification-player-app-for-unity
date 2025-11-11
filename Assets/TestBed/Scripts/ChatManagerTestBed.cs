using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using GamificationPlayer.DTO.ExternalEvents;

namespace GamificationPlayer.TestBed
{
    /// <summary>
    /// ChatManager Test Bed - Single MonoBehaviour for comprehensive ChatManager testing
    /// Drop this into any scene to instantly test ChatManager with staging environment
    /// </summary>
    public class ChatManagerTestBed : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private EnvironmentConfig environmentConfig;

        #region Core Components
        private ChatManager chatManager;
        private GamificationPlayerEndpoints endpoints;
        private ISessionLogData sessionData;
        #endregion

        #region Services
        private IChatAIService AIService;
        private IRAGService ragService;
        #endregion

        #region Chat State
        private string userInput = "";
        private bool isInitialized = false;
        private bool isConnected = false;
        private string connectionStatus = "Disconnected";
        #endregion

        #region Event Monitoring
        private List<string> eventLog = new List<string>();
        private Vector2 eventLogScrollPos;
        private Vector2 chatScrollPos;
        #endregion

        #region Performance Metrics
        private float lastResponseTime = 0f;
        private DateTime lastOperationStart;
        #endregion

        #region GUI Layout
        private Rect windowRect = new Rect(50, 50, 800, 600);
        private bool showDebugPanel = true;
        #endregion

        #region Unity Lifecycle

        void Start()
        {
            LogEvent("=== ChatManager Test Bed Starting ===");
            
            // Initialize components
            InitializeTestBed();
            
            if (autoInitialize)
            {
                StartCoroutine(AutoInitialize());
            }
        }

        void OnGUI()
        {
            // Main test bed window
            windowRect = GUI.Window(0, windowRect, DrawTestBedWindow, "ChatManager Test Bed - Staging Environment");
        }

        void OnDestroy()
        {
            CleanupEventHandlers();
        }

        #endregion

        #region Initialization

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
                
            }
            catch (Exception ex)
            {
                LogEvent($"Failed to initialize test bed: {ex.Message}");
                connectionStatus = "Error";
            }
        }

        private void SetupServiceOptions()
        {
            AIService = new ChatAIService("",
                isLoggingEnabled: enableLogging);
            
            // RAG service will be initialized asynchronously
            ragService = null;
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
            
            // Create Gamification Player endpoints (uses EnvironmentConfig for backend API)
            if (environmentConfig != null)
            {
                endpoints = new GamificationPlayerEndpoints(environmentConfig, sessionData);
            }
            
            // Create ChatManager (connects to Gamification Player backend)
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
            
            // Use async RAG initialization from Resources (WebGL compatible)
            // Note: Model loads from StreamingAssets, but config comes from Resources
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
                    // Continue without RAG service
                    ragService = null;
                }
                else if (initTask.Result)
                {
                    LogEvent("✅ RAG system initialized successfully");
                    ragService = RAGManager.CreateRAGService(enableLogging);
                    LogEvent($"RAG Status: {RAGManager.GetStatusInfo()}");
                }
                else
                {
                    LogEvent($"❌ RAG initialization failed: {RAGManager.LastError}");
                    ragService = null;
                }
            }
            catch (System.Exception ex)
            {
                LogEvent($"❌ RAG initialization error: {ex.Message}");
                ragService = null;
            }
        }

        #endregion

        #region Event Handlers

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

        private void OnChatInitialized()
        {
            LogEvent("✓ OnChatInitialized");
            isConnected = true;
            connectionStatus = "Connected";
            RecordPerformanceMetric("Chat Initialization");
        }

        private void OnMessageReceived(ChatManager.ChatMessage message)
        {
            LogEvent($"✓ OnMessageReceived: \"{message.message.Substring(0, Math.Min(50, message.message.Length))}...\"");
                        
            RecordPerformanceMetric("Message Received");
        }

        private void OnAIMessageReceived(ChatManager.ChatMessage message)
        {
            LogEvent($"✓ OnAIMessageReceived: \"{message.message.Substring(0, Math.Min(50, message.message.Length))}...\"");
                        
            RecordPerformanceMetric("AI Response Received");
        }

        private void OnAIMessageChunkReceived(string chunk)
        {
            LogEvent($"⚡ Stream chunk: \"{chunk.Substring(0, Math.Min(30, chunk.Length))}...\"");
        }

        private void OnErrorOccurred(string error)
        {
            LogEvent($"❌ OnErrorOccurred: {error}");
            connectionStatus = $"Error: {error}";
        }

        #endregion

        #region GUI Implementation

        private void DrawTestBedWindow(int windowID)
        {
            GUILayout.BeginVertical();
            
            // Header with configuration
            DrawHeaderPanel();
            
            GUILayout.BeginHorizontal();
            
            // Left panel: Chat interface
            GUILayout.BeginVertical(GUILayout.Width(400));
            DrawChatPanel();
            GUILayout.EndVertical();
            
            // Right panel: Controls and debug
            if (showDebugPanel)
            {
                GUILayout.BeginVertical(GUILayout.Width(380));
                DrawControlsPanel();
                DrawDebugPanel();
                GUILayout.EndVertical();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            GUI.DragWindow();
        }

        private void DrawHeaderPanel()
        {
            GUILayout.BeginHorizontal("box");
            
            GUILayout.FlexibleSpace();
            
            // Connection status
            GUI.color = isConnected ? Color.green : Color.yellow;
            GUILayout.Label($"● {connectionStatus}", GUILayout.Width(150));
            GUI.color = Color.white;

            GUILayout.EndHorizontal();
        }

        private void DrawChatPanel()
        {
            GUILayout.Label("Chat History");
            
            // Chat history area
            chatScrollPos = GUILayout.BeginScrollView(chatScrollPos, "box", GUILayout.Height(350));

            foreach (var msg in chatManager.GetConversationHistory())
            {
                GUILayout.BeginVertical("box");

                // Message header
                GUI.color = msg.role.Contains("agent") ? Color.cyan :
                           msg.role == "pre_defined" ? Color.green : Color.white;
                GUILayout.Label($"{msg.role} [{msg.timestamp:HH:mm:ss}]");
                GUI.color = Color.white;

                // Message content
                GUILayout.TextArea(msg.message);

                // Buttons if available
                if (msg.buttons != null && msg.buttons.Length > 0)
                {
                    GUILayout.BeginHorizontal();
                    foreach (var button in msg.buttons)
                    {
                        if (GUILayout.Button(button.text, GUILayout.Height(25)))
                        {
                            HandleButtonClick(button.identifier);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
            
            GUILayout.EndScrollView();
            
            // Input area
            GUILayout.BeginHorizontal();
            GUILayout.Label("Message:", GUILayout.Width(60));
            userInput = GUILayout.TextField(userInput);
            
            GUI.enabled = isConnected && !string.IsNullOrEmpty(userInput);
            if (GUILayout.Button("Send", GUILayout.Width(60)) || 
                (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == ""))
            {
                SendUserMessage();
            }
            GUI.enabled = true;
            
            GUILayout.EndHorizontal();
        }

        private void DrawControlsPanel()
        {
            GUILayout.Label("Controls & Debug");
            
            GUILayout.BeginVertical("box");
            
            // Main controls
            GUI.enabled = isInitialized;
            if (GUILayout.Button("Initialize Chat"))
            {
                StartChatInitialization();
            }
            GUI.enabled = true;
            
            if (GUILayout.Button("Force New Conversation"))
            {
                LogEvent("Force new conversation requested");
                chatManager.ForceNewConversation(AIService, ragService, new ChatManager.InitialMetadata("Herstel Buddy", "Frank", DateTime.Now));
            }
            
            GUILayout.Space(10);

            // Test activities
            GUILayout.Label("Test Activities:");
            Dictionary<string, string>[] testActivities =
            {
                new Dictionary<string, string> {
                    { "type", "animatie" },
                    { "name", "Adem in... adem uit" },
                    { "intro", "Doe je mee met wat ademwerk? Een paar minuten bewust ademhalen, zorgt direct voor nieuwe energie. Zet de video maar aan en volg de cirkel." },
                    { "post_activity_question", "Hoe was dat voor je?" },
                    { "context", "The user just completed a breathing exercise activity. Please ask the post_activity_question" }
                }
            };

            GUILayout.BeginHorizontal();
            foreach (var testActivity in testActivities )
            {
                if (GUILayout.Button(testActivity["name"]))
                {
                    HandleUserActivity(testActivity);
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Performance metrics
            GUILayout.Label("Performance:");
            GUILayout.Label($"Last Response: {lastResponseTime:F2}s");
            
            GUILayout.EndVertical();
        }

        private void DrawDebugPanel()
        {
            GUILayout.Label("Event Log");

            eventLogScrollPos = GUILayout.BeginScrollView(eventLogScrollPos, "box", GUILayout.Height(200));

            foreach (var logEntry in eventLog)
            {
                GUILayout.Label(logEntry);
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Log"))
            {
                eventLog.Clear();
            }

            if (GUILayout.Button("Export Log"))
            {
                ExportEventLog();
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Chat Operations

        private void StartChatInitialization()
        {
            if (chatManager == null || AIService == null)
            {
                LogEvent("❌ Cannot initialize: Missing ChatManager or AI service");
                return;
            }
            
            LogEvent("Starting chat initialization...");
            StartPerformanceTimer();

            chatManager.InitializeChat(AIService, ragService, new ChatManager.ResumeConversationMetadata(), new ChatManager.InitialMetadata("Herstel Buddy", "Frank", DateTime.Now));
        }

        private void SendUserMessage()
        {
            if (string.IsNullOrEmpty(userInput) || AIService == null || ragService == null)
                return;
            
            string message = userInput;
            userInput = "";
            
            LogEvent($"📤 Sending user message: {message}");
            
            StartPerformanceTimer();
            chatManager.HandleUserMessage(AIService, ragService, message);
        }

        private void HandleButtonClick(string buttonId)
        {
            if (AIService == null)
            {
                LogEvent("❌ Cannot handle button click: Missing AI service");
                return;
            }

            LogEvent($"🔘 Button clicked: {buttonId}");

            StartPerformanceTimer();
            chatManager.HandleButtonClick(AIService, buttonId);
        }
        
        private void HandleUserActivity(Dictionary<string, string> userActivityMetadata)
        {
            if (AIService == null)
            {
                LogEvent("❌ Cannot handle user activity: Missing AI service");
                return;
            }

            LogEvent($"🔘 User activity detected: {userActivityMetadata.ToJson()}");

            StartPerformanceTimer();
            chatManager.HandleUserActivity(AIService, ragService, userActivityMetadata);
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
                Debug.Log($"[TestBed] {message}");
            }
        }

        private void ExportEventLog()
        {
            try
            {
                string filename = $"ChatTestBed_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string path = System.IO.Path.Combine(Application.persistentDataPath, filename);
                
                System.IO.File.WriteAllLines(path, eventLog);
                
                LogEvent($"Event log exported to: {path}");
                Debug.Log($"Event log exported to: {path}");
            }
            catch (Exception ex)
            {
                LogEvent($"Failed to export log: {ex.Message}");
            }
        }

        #endregion
    }
}