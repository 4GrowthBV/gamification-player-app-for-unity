using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using GamificationPlayer;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using GamificationPlayer.Tests;
using GamificationPlayer.TestBed.ProductionServices;
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
        [SerializeField] private bool useStagingEnvironment = true;
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool autoInitialize = true;

        #region Core Components
        private ChatManager chatManager;
        private GamificationPlayerEndpoints endpoints;
        private ISessionLogData sessionData;
        private EnvironmentConfig environmentConfig;
        #endregion

        #region Services
        private IChatAIService currentAIService;
        private IChatRouterService currentRouterService;
        
        // Service options
        private List<ServiceOption<IChatAIService>> aiServiceOptions = new List<ServiceOption<IChatAIService>>();
        private List<ServiceOption<IChatRouterService>> routerServiceOptions = new List<ServiceOption<IChatRouterService>>();
        private int selectedAIServiceIndex = 0;
        private int selectedRouterServiceIndex = 0;
        #endregion

        #region Chat State
        private List<ChatDisplayMessage> chatHistory = new List<ChatDisplayMessage>();
        private string userInput = "";
        private string[] availableButtons = new string[0];
        private bool isInitialized = false;
        private bool isConnected = false;
        private string connectionStatus = "Disconnected";
        private string currentConversationId = "";
        private string currentProfileId = "";
        #endregion

        #region Event Monitoring
        private List<string> eventLog = new List<string>();
        private Vector2 eventLogScrollPos;
        private Vector2 chatScrollPos;
        #endregion

        #region Performance Metrics
        private float lastResponseTime = 0f;
        private int totalAPICallsCount = 0;
        private DateTime lastOperationStart;
        private StringBuilder performanceLog = new StringBuilder();
        #endregion

        #region GUI Layout
        private Rect windowRect = new Rect(50, 50, 800, 600);
        private bool showDebugPanel = true;
        private bool isRecording = false;
        #endregion

        [Serializable]
        private class ChatDisplayMessage
        {
            public string sender;
            public string message;
            public string[] buttons;
            public DateTime timestamp;
            public bool isStreaming;

            public ChatDisplayMessage(string sender, string message, string[] buttons = null)
            {
                this.sender = sender;
                this.message = message;
                this.buttons = buttons;
                this.timestamp = DateTime.Now;
                this.isStreaming = false;
            }
        }

        private class ServiceOption<T>
        {
            public string name;
            public System.Func<T> createService;
            public bool isProduction;

            public ServiceOption(string name, System.Func<T> createService, bool isProduction = false)
            {
                this.name = name;
                this.createService = createService;
                this.isProduction = isProduction;
            }
        }

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
                // Load environment configuration
                LoadEnvironmentConfig();
                
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

        private void LoadEnvironmentConfig()
        {
            // Load Gamification Player environment config (for ChatManager backend API)
            // Note: This is separate from OpenAI/N8n configurations
            var configs = AssetDatabase.FindAssets("t:EnvironmentConfig LearnStrikeEnviromentConfigStaging");
            if (configs.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(configs[0]);
                environmentConfig = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(path);
                LogEvent($"Loaded Gamification Player staging config successfully");
            }
            else
            {
                LogEvent("Warning: Gamification Player staging environment config not found!");
            }
        }

        private void SetupServiceOptions()
        {
            // AI Service Options
            aiServiceOptions.Clear();
            aiServiceOptions.Add(new ServiceOption<IChatAIService>("OpenAI Mock Service", 
                () => gameObject.AddComponent<OpenAIChatMockService>(), false));
            aiServiceOptions.Add(new ServiceOption<IChatAIService>("OpenAI Production", 
                () => gameObject.AddComponent<ProductionServices.OpenAIChatService>(), true));
            
            // Router Service Options
            routerServiceOptions.Clear();
            routerServiceOptions.Add(new ServiceOption<IChatRouterService>("N8n Mock Service", 
                () => gameObject.AddComponent<N8nRouterMockService>(), false));
            routerServiceOptions.Add(new ServiceOption<IChatRouterService>("N8n Production", 
                () => gameObject.AddComponent<N8nRouterServiceTest>(), true));
            
            // Initialize with production services if using staging environment
            if (useStagingEnvironment)
            {
                selectedAIServiceIndex = 1; // Production OpenAI
                selectedRouterServiceIndex = 1; // Production N8n
            }
            
            // Initialize with first options
            RefreshServices();
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
                chatManager.Initialize(endpoints, sessionData);
                isInitialized = chatManager.IsInitialized();
            }
        }

        private IEnumerator AutoInitialize()
        {
            yield return new WaitForSeconds(0.5f); // Let everything settle
            
            if (isInitialized)
            {
                LogEvent("Auto-initializing chat...");
                StartChatInitialization();
            }
        }

        #endregion

        #region Service Management

        private void RefreshServices()
        {
            // Clean up existing services
            if (currentAIService != null && currentAIService is MonoBehaviour aiMono)
            {
                DestroyImmediate(aiMono);
            }
            if (currentRouterService != null && currentRouterService is MonoBehaviour routerMono)
            {
                DestroyImmediate(routerMono);
            }
            
            // Create new services
            if (selectedAIServiceIndex < aiServiceOptions.Count)
            {
                currentAIService = aiServiceOptions[selectedAIServiceIndex].createService();
                LogEvent($"Switched to AI service: {aiServiceOptions[selectedAIServiceIndex].name}");
            }
            
            if (selectedRouterServiceIndex < routerServiceOptions.Count)
            {
                currentRouterService = routerServiceOptions[selectedRouterServiceIndex].createService();
                LogEvent($"Switched to Router service: {routerServiceOptions[selectedRouterServiceIndex].name}");
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

        private void OnMessageReceived(string message, string[] buttons)
        {
            LogEvent($"✓ OnMessageReceived: \"{message.Substring(0, Math.Min(50, message.Length))}...\"");
            
            var displayMsg = new ChatDisplayMessage("Bot", message, buttons);
            chatHistory.Add(displayMsg);
            
            availableButtons = buttons ?? new string[0];
            
            RecordPerformanceMetric("Message Received");
        }

        private void OnAIMessageReceived(string response)
        {
            LogEvent($"✓ OnAIMessageReceived: \"{response.Substring(0, Math.Min(50, response.Length))}...\"");
            
            var displayMsg = new ChatDisplayMessage("AI", response);
            chatHistory.Add(displayMsg);
            
            availableButtons = new string[0]; // AI messages don't have buttons
            
            RecordPerformanceMetric("AI Response Received");
        }

        private void OnAIMessageChunkReceived(string chunk)
        {
            LogEvent($"⚡ Stream chunk: \"{chunk.Substring(0, Math.Min(30, chunk.Length))}...\"");
            
            // Update the last message if it's streaming, or create new one
            if (chatHistory.Count > 0 && chatHistory[chatHistory.Count - 1].isStreaming)
            {
                chatHistory[chatHistory.Count - 1].message = chunk;
            }
            else
            {
                var streamMsg = new ChatDisplayMessage("AI (Streaming)", chunk) { isStreaming = true };
                chatHistory.Add(streamMsg);
            }
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
            
            // Environment status
            GUILayout.Label($"Config: {(useStagingEnvironment ? "Staging" : "Mock")}", GUILayout.Width(100));
            
            // Service selection
            GUILayout.Label("AI:", GUILayout.Width(25));
            int newAIIndex = EditorGUILayout.Popup(selectedAIServiceIndex, GetServiceNames(aiServiceOptions), GUILayout.Width(120));
            if (newAIIndex != selectedAIServiceIndex)
            {
                selectedAIServiceIndex = newAIIndex;
                RefreshServices();
            }
            
            GUILayout.Label("Router:", GUILayout.Width(45));
            int newRouterIndex = EditorGUILayout.Popup(selectedRouterServiceIndex, GetServiceNames(routerServiceOptions), GUILayout.Width(120));
            if (newRouterIndex != selectedRouterServiceIndex)
            {
                selectedRouterServiceIndex = newRouterIndex;
                RefreshServices();
            }
            
            GUILayout.FlexibleSpace();
            
            // Connection status
            GUI.color = isConnected ? Color.green : Color.yellow;
            GUILayout.Label($"● {connectionStatus}", GUILayout.Width(150));
            GUI.color = Color.white;
            
            GUILayout.EndHorizontal();
            
            // IDs row
            GUILayout.BeginHorizontal("box");
            GUILayout.Label($"Conv: {currentConversationId}", GUILayout.Width(200));
            GUILayout.Label($"Profile: {currentProfileId}", GUILayout.Width(200));
            
            if (GUILayout.Button(showDebugPanel ? "Hide Debug" : "Show Debug", GUILayout.Width(100)))
            {
                showDebugPanel = !showDebugPanel;
                windowRect.width = showDebugPanel ? 800 : 420;
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawChatPanel()
        {
            GUILayout.Label("Chat History", EditorStyles.boldLabel);
            
            // Chat history area
            chatScrollPos = GUILayout.BeginScrollView(chatScrollPos, "box", GUILayout.Height(350));
            
            foreach (var msg in chatHistory)
            {
                GUILayout.BeginVertical("box");
                
                // Message header
                GUI.color = msg.sender.Contains("AI") ? Color.cyan : 
                           msg.sender == "Bot" ? Color.green : Color.white;
                GUILayout.Label($"{msg.sender} [{msg.timestamp:HH:mm:ss}]", EditorStyles.boldLabel);
                GUI.color = Color.white;
                
                // Message content
                GUILayout.TextArea(msg.message, EditorStyles.wordWrappedLabel);
                
                // Buttons if available
                if (msg.buttons != null && msg.buttons.Length > 0)
                {
                    GUILayout.BeginHorizontal();
                    foreach (var button in msg.buttons)
                    {
                        if (GUILayout.Button(button, GUILayout.Height(25)))
                        {
                            HandleButtonClick(button);
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
            
            if (GUILayout.Button(isRecording ? "●Stop" : "⚫Rec", GUILayout.Width(50)))
            {
                isRecording = !isRecording;
            }
            
            GUILayout.EndHorizontal();
        }

        private void DrawControlsPanel()
        {
            GUILayout.Label("Controls & Debug", EditorStyles.boldLabel);
            
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
                // TODO: Implement force new conversation
                LogEvent("Force new conversation requested");
            }
            
            if (GUILayout.Button("Clear History"))
            {
                chatHistory.Clear();
                LogEvent("Chat history cleared");
            }
            
            GUILayout.Space(10);
            
            // Test buttons
            GUILayout.Label("Test Buttons:", EditorStyles.boldLabel);
            string[] testButtons = { "day_one", "day_two", "test_button", "welcome" };
            
            GUILayout.BeginHorizontal();
            foreach (var testBtn in testButtons)
            {
                if (GUILayout.Button(testBtn))
                {
                    HandleButtonClick(testBtn);
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Performance metrics
            GUILayout.Label("Performance:", EditorStyles.boldLabel);
            GUILayout.Label($"Last Response: {lastResponseTime:F2}s");
            GUILayout.Label($"Total API Calls: {totalAPICallsCount}");
            
            GUILayout.EndVertical();
        }

        private void DrawDebugPanel()
        {
            GUILayout.Label("Event Log", EditorStyles.boldLabel);
            
            eventLogScrollPos = GUILayout.BeginScrollView(eventLogScrollPos, "box", GUILayout.Height(200));
            
            foreach (var logEntry in eventLog)
            {
                GUILayout.Label(logEntry, EditorStyles.wordWrappedLabel);
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

        private string[] GetServiceNames<T>(List<ServiceOption<T>> options)
        {
            string[] names = new string[options.Count];
            for (int i = 0; i < options.Count; i++)
            {
                names[i] = options[i].name + (options[i].isProduction ? " (Prod)" : " (Mock)");
            }
            return names;
        }

        #endregion

        #region Chat Operations

        private void StartChatInitialization()
        {
            if (chatManager == null || currentAIService == null)
            {
                LogEvent("❌ Cannot initialize: Missing ChatManager or AI service");
                return;
            }
            
            LogEvent("Starting chat initialization...");
            StartPerformanceTimer();
            
            chatManager.InitializeChat(currentAIService);
        }

        private void SendUserMessage()
        {
            if (string.IsNullOrEmpty(userInput) || currentAIService == null || currentRouterService == null)
                return;
            
            string message = userInput;
            userInput = "";
            
            LogEvent($"📤 Sending user message: {message}");
            
            // Add to chat display immediately
            chatHistory.Add(new ChatDisplayMessage("User", message));
            
            StartPerformanceTimer();
            chatManager.HandleUserMessage(currentAIService, currentRouterService, message);
        }

        private void HandleButtonClick(string buttonId)
        {
            if (currentAIService == null)
            {
                LogEvent("❌ Cannot handle button click: Missing AI service");
                return;
            }
            
            LogEvent($"🔘 Button clicked: {buttonId}");
            
            // Add to chat display
            chatHistory.Add(new ChatDisplayMessage("User", $"[Button: {buttonId}]"));
            
            StartPerformanceTimer();
            chatManager.HandleButtonClick(currentAIService, buttonId);
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
            totalAPICallsCount++;
            
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