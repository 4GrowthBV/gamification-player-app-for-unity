using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GamificationPlayer;
using GamificationPlayer.Session;
using GamificationPlayer.AI;
using UnityEditor;
using GamificationPlayer.DTO.ExternalEvents;

namespace GamificationPlayer.Testing
{
    /// <summary>
    /// Test bed for ChatManager with GUI controls for play mode testing
    /// Creates its own ChatManager instance with mock dependencies
    /// </summary>
    public class ChatManagerTestBed : MonoBehaviour
    {
        [Header("Chat Manager Instance")]
        [SerializeField] private ChatManager chatManager;
        
        [Header("Test Configuration")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private Vector2 windowSize = new Vector2(600, 800);
        [SerializeField] private Vector2 windowPosition = new Vector2(50, 50);
        
        [Header("AI Integration")]
        [SerializeField] private bool enableAIResponses = true;
        [SerializeField] private string openAIApiKey = "";
        [SerializeField] private string openAIModel = "gpt-4o-mini";
        [SerializeField] private string n8nRAGEndpoint = "";
        [SerializeField] private string n8nApiKey = "";
        [SerializeField] private bool enableParallelProcessing = true;
        
        // Mock dependencies
        private GamificationPlayerEndpoints mockEndpoints;
        private SessionLogData mockSessionData;
        private EnvironmentConfig environmentConfig;
        
        // AI Components
        private AIAgent aiAgent;
        private OpenAIClient.Config openAIConfig;
        private N8nRAGClient.Config ragConfig;
        private AIAgent.Config aiConfig;
        private AITestBedConfig aiTestBedConfig;
        
        // GUI State
        private string userInputText = "";
        private string buttonInputText = "start-button";
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 logScrollPosition = Vector2.zero;
        private List<string> logMessages = new List<string>();
        private bool showChatHistory = true;
        private bool showLogs = false;
        private bool showControls = false;
        private bool showAIConfig = false;
        private Rect windowRect;
        
        // Chat UI State
        private List<ChatUIMessage> chatUIMessages = new List<ChatUIMessage>();
        private bool waitingForAIResponse = false;
        private string currentProcessingStatus = "";
        private string currentUserProfile = "";
        private int lastTokensUsed = 0;
        private float lastProcessingTime = 0f;
        
        [System.Serializable]
        public class ChatUIMessage
        {
            public string role; // "user", "bot", "system"
            public string message;
            public string[] buttons;
            public DateTime timestamp;
            public bool isButtonResponse; // True if this was a button click response
            
            public ChatUIMessage(string role, string message, string[] buttons = null, bool isButtonResponse = false)
            {
                this.role = role;
                this.message = message;
                this.buttons = buttons ?? new string[0];
                this.timestamp = DateTime.Now;
                this.isButtonResponse = isButtonResponse;
            }
        }
        
        // Note: Using API-based conversation detection instead of mock persistence
        
        // Chat State
        private List<ChatManager.ChatMessage> currentHistory = new List<ChatManager.ChatMessage>();
        private string lastReceivedMessage = "";
        private string[] lastReceivedButtons = new string[0];
        private string lastAIMessage = "";
        private string lastError = "";
        private bool chatInitialized = false;

        void Start()
        {
            windowRect = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);
            
            // Create ChatManager instance if not assigned
            if (chatManager == null)
            {
                CreateChatManagerInstance();
            }
            
            // Subscribe to ChatManager events
            SubscribeToEvents();
            
            // Load AI configuration and initialize components
            LoadAIConfiguration();
            InitializeAIComponents();
            
            // Auto-initialize if requested
            if (autoInitialize)
            {
                InitializeChatManager();
                // Clear UI messages before starting
                chatUIMessages.Clear();
                // Start chat flow after initialization
                StartCoroutine(StartInitialChatFlow());
            }
            
            LogMessage("ChatManager Test Bed initialized");
        }
        
        private System.Collections.IEnumerator StartInitialChatFlow()
        {
            // Wait a moment for initialization to complete
            yield return new WaitForSeconds(0.5f);
            
            if (chatManager != null && chatManager.IsInitialized())
            {
                // Initialize chat (this will handle existing conversations automatically)
                chatManager.InitializeChat();
                LogMessage("Started chat initialization - will resume existing conversation or start new one");
                
                // Don't manually call day_one - let ChatManager handle it based on conversation state
                // ChatManager will either:
                // 1. Load existing conversation history, or 
                // 2. Start with day_one if it's a new conversation
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromEvents();
            UnsubscribeFromAIEvents();
        }

        void OnGUI()
        {
            GUI.skin.window.fontSize = 12;
            GUI.skin.label.fontSize = 11;
            GUI.skin.button.fontSize = 11;
            GUI.skin.textField.fontSize = 11;
            GUI.skin.textArea.fontSize = 10;
            
            windowRect = GUI.Window(0, windowRect, DrawWindow, "ChatManager Test Bed");
        }

        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();
            
            // Header with status
            DrawStatusSection();
            
            GUILayout.Space(10);
            
            // Main chat interface
            DrawChatInterface();
            
            GUILayout.Space(10);
            
            // Control buttons (collapsed by default)
            if (showControls)
            {
                DrawControlSection();
                GUILayout.Space(10);
            }
            
            // AI Configuration section (collapsed by default)
            if (showAIConfig)
            {
                DrawAIConfigSection();
                GUILayout.Space(10);
            }
            
            // Debug logs (collapsed by default)
            if (showLogs)
            {
                DrawLogsSection();
            }
            
            GUILayout.EndVertical();
            
            // Make window draggable
            GUI.DragWindow();
        }

        void DrawStatusSection()
        {
            GUILayout.Label("=== STATUS ===", GUI.skin.box);
            
            // ChatManager status
            string chatStatus = chatManager != null ? 
                (chatManager.IsInitialized() ? "✓ Initialized" : "✗ Not Initialized") : 
                "✗ No ChatManager";
            GUILayout.Label($"ChatManager: {chatStatus}");
            
            // Mock dependencies status
            string mockStatus = (mockEndpoints != null && mockSessionData != null) ? "✓ Created" : "✗ Not Created";
            GUILayout.Label($"Mock Dependencies: {mockStatus}");
            
            // Chat state
            if (chatManager != null && chatManager.IsInitialized())
            {
                GUILayout.Label($"In Predefined Flow: {chatManager.IsInPredefinedFlow()}");
                GUILayout.Label($"History Count: {currentHistory.Count}");
                GUILayout.Label($"Chat Initialized: {chatInitialized}");
            }
            
            // AI Status
            string aiStatus = aiAgent != null ? "✓ Ready" : "✗ Not Configured";
            GUILayout.Label($"AI Agent: {aiStatus}");
            
            if (enableAIResponses && aiAgent != null)
            {
                bool hasOpenAI = !string.IsNullOrEmpty(openAIApiKey);
                bool hasRAG = !string.IsNullOrEmpty(n8nRAGEndpoint);
                
                GUILayout.Label($"OpenAI: {(hasOpenAI ? "✓" : "✗")} | RAG: {(hasRAG ? "✓" : "✗")}");
                
                if (!string.IsNullOrEmpty(currentProcessingStatus))
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label($"Status: {currentProcessingStatus}");
                    GUI.color = Color.white;
                }
                
                if (lastTokensUsed > 0)
                {
                    GUILayout.Label($"Last Response: {lastTokensUsed} tokens, {lastProcessingTime:F2}s");
                }
            }
            
            // API-based conversation controls
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Force New", GUILayout.Width(100)))
            {
                ForceNewConversation();
            }
            if (GUILayout.Button("📜 Check API", GUILayout.Width(100)))
            {
                ResumeExistingConversation();
            }
            if (GUILayout.Button("� Reset TestBed", GUILayout.Width(120)))
            {
                ClearPersistedData();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Toggle sections
            GUILayout.BeginHorizontal();
            showControls = GUILayout.Toggle(showControls, "Debug Controls", GUILayout.Width(100));
            showLogs = GUILayout.Toggle(showLogs, "Debug Logs", GUILayout.Width(100));
            showAIConfig = GUILayout.Toggle(showAIConfig, "AI Config", GUILayout.Width(80));
            GUILayout.EndHorizontal();
        }
        
        void DrawChatInterface()
        {
            GUILayout.Label("=== CHAT INTERFACE ===", GUI.skin.box);
            
            // Chat messages area
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            foreach (var message in chatUIMessages)
            {
                DrawChatMessage(message);
                GUILayout.Space(5);
            }
            
            // Show waiting indicator
            if (waitingForAIResponse)
            {
                GUI.color = Color.yellow;
                GUILayout.Label("🤖 AI is thinking...");
                GUI.color = Color.white;
            }
            
            GUILayout.EndScrollView();
            
            // User input area
            GUILayout.Space(10);
            DrawUserInputArea();
        }
        
        void DrawChatMessage(ChatUIMessage message)
        {
            // Message container with background
            GUIStyle messageStyle = new GUIStyle(GUI.skin.box);
            
            if (message.role == "user")
            {
                messageStyle.normal.textColor = Color.white;
                GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f, 0.8f); // Green for user
            }
            else if (message.role == "bot")
            {
                messageStyle.normal.textColor = Color.white;
                GUI.backgroundColor = new Color(0.2f, 0.5f, 0.8f, 0.8f); // Blue for bot
            }
            else // system
            {
                messageStyle.normal.textColor = Color.white;
                GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 0.8f); // Gray for system
            }
            
            GUILayout.BeginVertical(messageStyle);
            
            // Message header
            GUILayout.BeginHorizontal();
            string roleIcon = message.role == "user" ? "👤" : (message.role == "bot" ? "🤖" : "ℹ️");
            GUILayout.Label($"{roleIcon} {message.role.ToUpper()}", GUILayout.Width(80));
            GUILayout.Label(message.timestamp.ToString("HH:mm:ss"), GUILayout.Width(60));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            // Message content
            GUILayout.Label(message.message, GUI.skin.textArea);
            
            // Buttons (if any)
            if (message.buttons != null && message.buttons.Length > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("Available responses:");
                
                // Display buttons in rows of 2
                for (int i = 0; i < message.buttons.Length; i += 2)
                {
                    GUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button(message.buttons[i], GUILayout.Height(30)))
                    {
                        OnButtonClicked(message.buttons[i]);
                    }
                    
                    if (i + 1 < message.buttons.Length)
                    {
                        if (GUILayout.Button(message.buttons[i + 1], GUILayout.Height(30)))
                        {
                            OnButtonClicked(message.buttons[i + 1]);
                        }
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }
        
        void DrawUserInputArea()
        {
            GUILayout.Label("💬 Your message:");
            
            GUILayout.BeginHorizontal();
            
            // Text input
            userInputText = GUILayout.TextField(userInputText, GUILayout.Height(25));
            
            // Send button
            bool canSend = !string.IsNullOrEmpty(userInputText) && !waitingForAIResponse;
            GUI.enabled = canSend;
            
            if (GUILayout.Button("Send", GUILayout.Width(60), GUILayout.Height(25)) || 
                (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && canSend))
            {
                SendUserMessage();
            }
            
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            
            // Quick action buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("😰 I'm stressed", GUILayout.Height(25)))
            {
                userInputText = "I'm feeling really stressed lately";
            }
            if (GUILayout.Button("⚖️ Legal help", GUILayout.Height(25)))
            {
                userInputText = "I need legal advice about my rights";
            }
            if (GUILayout.Button("💼 Work issue", GUILayout.Height(25)))
            {
                userInputText = "I have an issue with HR at work";
            }
            GUILayout.EndHorizontal();
        }
        
        void OnButtonClicked(string buttonId)
        {
            // Add user's button choice to chat
            AddChatMessage("user", $"[Selected: {buttonId}]", null, true);
            
            // Send button click to ChatManager
            if (chatManager != null)
            {
                chatManager.HandleButtonClick(buttonId);
                LogMessage($"User clicked button: {buttonId}");
            }
        }
        
        void SendUserMessage()
        {
            if (string.IsNullOrEmpty(userInputText) || waitingForAIResponse)
                return;
                
            string message = userInputText;
            userInputText = ""; // Clear input
            
            // Add user message to chat
            AddChatMessage("user", message);
            
            // Set waiting state
            waitingForAIResponse = true;
            
            // Check if we should use AI responses or ChatManager
            if (enableAIResponses && aiAgent != null && !string.IsNullOrEmpty(openAIApiKey))
            {
                // Use AI Agent for response
                ProcessWithAI(message);
            }
            else
            {
                // Send to ChatManager (original behavior)
                if (chatManager != null)
                {
                    chatManager.HandleUserMessage(message);
                    LogMessage($"User sent message: {message}");
                }
            }
        }
        
        void AddChatMessage(string role, string message, string[] buttons = null, bool isButtonResponse = false)
        {
            var chatMessage = new ChatUIMessage(role, message, buttons, isButtonResponse);
            chatUIMessages.Add(chatMessage);
            
            // Limit chat history to prevent memory issues
            if (chatUIMessages.Count > 100)
            {
                chatUIMessages.RemoveAt(0);
            }
            
            // Auto-scroll to bottom
            scrollPosition.y = float.MaxValue;
        }

        void DrawControlSection()
        {
            GUILayout.Label("=== DEBUG CONTROLS ===", GUI.skin.box);
            
            // System controls
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Chat", GUILayout.Width(100)))
            {
                ResetChatInterface();
            }
            if (GUILayout.Button("Clear Logs", GUILayout.Width(100)))
            {
                logMessages.Clear();
            }
            if (GUILayout.Button("Restart Flow", GUILayout.Width(100)))
            {
                RestartChatFlow();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Force New Conversation", GUILayout.Width(200)))
            {
                ForceNewConversation();
            }
            if (GUILayout.Button("📜 Resume Existing", GUILayout.Width(150)))
            {
                ResumeExistingConversation();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Manual button testing
            GUILayout.Label("Manual Button Testing:");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Button ID:", GUILayout.Width(70));
            buttonInputText = GUILayout.TextField(buttonInputText, GUILayout.Width(150));
            if (GUILayout.Button("Send", GUILayout.Width(60)))
            {
                if (chatManager != null && !string.IsNullOrEmpty(buttonInputText))
                {
                    OnButtonClicked(buttonInputText);
                }
            }
            GUILayout.EndHorizontal();
            
            // Debug info
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get State Info", GUILayout.Width(120)))
            {
                if (chatManager != null)
                {
                    string stateInfo = chatManager.GetChatStateInfo();
                    AddChatMessage("system", $"📊 State: {stateInfo}");
                }
            }
            if (GUILayout.Button("Show Instructions", GUILayout.Width(120)))
            {
                ShowCurrentInstructions();
            }
            GUILayout.EndHorizontal();
        }
        
        void ResetChatInterface()
        {
            chatUIMessages.Clear();
            currentHistory.Clear();
            waitingForAIResponse = false;
            
            if (chatManager != null)
            {
                chatManager.ResetChat();
            }
            
            AddChatMessage("system", "🔄 Chat interface reset. Starting fresh...");
            LogMessage("Chat interface reset");
            
            // Restart the flow
            StartCoroutine(StartInitialChatFlow());
        }
        
        void RestartChatFlow()
        {
            ResetChatInterface();
            
            // Reinitialize everything
            InitializeChatManager();
            
            LogMessage("Restarting complete chat flow");
        }
        
        void ForceNewConversation()
        {
            if (chatManager != null && chatManager.IsInitialized())
            {
                // Reset the chat manager state
                chatManager.ResetChat();
                
                // Clear UI messages
                chatUIMessages.Clear();
                
                // Add system message before starting new conversation
                AddChatMessage("system", "🔄 Forcing new conversation - will create fresh chat session via API (ignoring any existing conversation)");
                LogMessage("Forcing new conversation with forceNewConversation=true (API-based)");
                
                // Force create new conversation (ignores any existing conversation found via API)
                chatManager.InitializeChat(true);
            }
            else
            {
                LogMessage("ChatManager not initialized, cannot force new conversation");
            }
        }
        
        void ResumeExistingConversation()
        {
            if (chatManager != null && chatManager.IsInitialized())
            {
                // Reset the chat manager state
                chatManager.ResetChat();
                
                // Clear UI messages
                chatUIMessages.Clear();
                
                // Add system message before resuming
                AddChatMessage("system", "📜 Checking for existing conversation via API - will resume if found, create new if not");
                LogMessage("Resuming existing conversation with forceNewConversation=false (API-based detection)");
                
                // Check for existing conversation via API first, create new if none found
                chatManager.InitializeChat(false);
            }
            else
            {
                LogMessage("ChatManager not initialized, cannot resume conversation");
            }
        }
        
        void ShowCurrentInstructions()
        {
            // This would show what instructions the AI is currently using
            string instructionInfo = "📋 Current AI Instructions:\n";
            instructionInfo += "- Predefined Flow: Responds with structured messages and buttons\n";
            instructionInfo += "- AI Flow: Uses agent routing (mindfulness, legal, HR, general)\n";
            instructionInfo += "- Memory Service: Updates user profile based on conversations\n";
            instructionInfo += "- Router Service: Determines appropriate agent based on user input";
            
            AddChatMessage("system", instructionInfo);
        }



        void DrawLogsSection()
        {
            GUILayout.Label("=== DEBUG LOGS ===", GUI.skin.box);
            
            logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Height(150));
            
            foreach (string log in logMessages)
            {
                if (log.Contains("ERROR") || log.Contains("Error"))
                {
                    GUI.color = Color.red;
                    GUILayout.Label(log);
                    GUI.color = Color.white;
                }
                else if (log.Contains("WARNING") || log.Contains("Warning"))
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label(log);
                    GUI.color = Color.white;
                }
                else
                {
                    GUILayout.Label(log);
                }
            }
            
            GUILayout.EndScrollView();
        }

        void CreateChatManagerInstance()
        {
            // Clean up existing instance if any
            if (chatManager != null)
            {
                DestroyImmediate(chatManager.gameObject);
            }
            
            // Create new ChatManager GameObject
            GameObject chatManagerGO = new GameObject("ChatManager (TestBed)");
            chatManagerGO.transform.SetParent(this.transform);
            chatManager = chatManagerGO.AddComponent<ChatManager>();
            
            LogMessage("Created new ChatManager instance");
        }

        void InitializeChatManager()
        {
            if (chatManager == null)
            {
                CreateChatManagerInstance();
            }
            
            // Create mock dependencies
            CreateMockDependencies();
            
            if (mockEndpoints == null || mockSessionData == null)
            {
                LogMessage("ERROR: Failed to create mock dependencies!");
                return;
            }
            
            // Initialize ChatManager with mock dependencies
            chatManager.Initialize(mockEndpoints, mockSessionData);
            
            if (chatManager.IsInitialized())
            {
                LogMessage("✓ ChatManager initialized successfully with mock dependencies");
            }
            else
            {
                LogMessage("ERROR: ChatManager initialization failed");
            }
            
            RefreshChatHistory();
        }

        void CreateMockDependencies()
        {
            try
            {
                // Create mock SessionLogData with realistic test data
                mockSessionData = new SessionLogData();
                LogMessage("Created mock SessionLogData");
                
                // Simulate persistence by loading previously stored conversation/profile IDs
                LoadPersistedChatData();

                // Add realistic test organization and microgame data
                mockSessionData.AddToLog(new MicroGamePayload()
                {
                    micro_game = new MicroGamePayload.MicroGame()
                    {
                        id = "99d75cfb-ce23-4939-a755-013d04a435c8",
                        name = "ChatManager TestBed"
                    },
                    organisation = new MicroGamePayload.Organisation()
                    {
                        id = "edb5e165-1c74-44f8-8d57-c24b82f2f5f2",
                        name = "Test Organisation"
                    },
                    player = new MicroGamePayload.Player()
                    {
                        user_id = "5b411dd2-20c1-49dd-90a5-555dbaead5f8"
                    }

                });

                LoadEnvironmentConfig();

                // Create mock GamificationPlayerEndpoints
                mockEndpoints = new GamificationPlayerEndpoints(environmentConfig, mockSessionData);
                LogMessage("Created mock GamificationPlayerEndpoints");
                
                // Log the endpoint creation for database tracking
                AddChatMessage("system", "🔗 Connected to chat endpoints:\n" +
                                       "• Conversations: Ready for session tracking\n" +
                                       "• Messages: Ready for message logging\n" +
                                       "• Profiles: Ready for AI profile updates");
            }
            catch (System.Exception ex)
            {
                LogMessage($"ERROR creating mock dependencies: {ex.Message}");
                AddChatMessage("system", $"❌ Failed to create dependencies: {ex.Message}");
            }
        }

        private void LoadEnvironmentConfig()
        {
            // Try to find existing EnvironmentConfig asset
            var configs = AssetDatabase.FindAssets("t:EnvironmentConfig GamificationPlayerEnviromentConfigStaging");
            if (configs.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(configs[0]);
                environmentConfig = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(path);
            }
        }
        
        private void LoadPersistedChatData()
        {
            // Let ChatManager handle conversation/profile checking via API
            // Remove the mock data injection to allow proper API-based conversation detection
            LogMessage("Skipping mock data injection - letting ChatManager check for existing conversations via API");
        }
        

        
        private void ClearPersistedData()
        {
            // Since we're now using API-based conversation detection,
            // clearing data means forcing a fresh conversation
            AddChatMessage("system", "🗑️ Will force new conversation on next initialization (API-based detection)");
            LogMessage("Clear data requested - will force new conversation via API");
            
            // Just reset the TestBed and force new conversation
            ForceNewConversation();
        }
        
        void RefreshChatHistory()
        {
            if (chatManager != null)
            {
                currentHistory = chatManager.GetConversationHistory();
                LogMessage($"Refreshed chat history: {currentHistory.Count} messages");
            }
        }

        void SubscribeToEvents()
        {
            ChatManager.OnMessageReceived += OnMessageReceived;
            ChatManager.OnAIMessageReceived += OnAIMessageReceived;
            ChatManager.OnErrorOccurred += OnErrorOccurred;
            ChatManager.OnChatInitialized += OnChatInitialized;
        }

        void UnsubscribeFromEvents()
        {
            ChatManager.OnMessageReceived -= OnMessageReceived;
            ChatManager.OnAIMessageReceived -= OnAIMessageReceived;
            ChatManager.OnErrorOccurred -= OnErrorOccurred;
            ChatManager.OnChatInitialized -= OnChatInitialized;
        }

        void OnMessageReceived(string message, string[] buttons)
        {
            lastReceivedMessage = message;
            lastReceivedButtons = buttons ?? new string[0];
            
            // Add bot message to chat interface
            AddChatMessage("bot", message, buttons);
            
            // Stop waiting indicator
            waitingForAIResponse = false;
            
            RefreshChatHistory();
            
            string buttonInfo = buttons != null && buttons.Length > 0 ? 
                $" with {buttons.Length} buttons: [{string.Join(", ", buttons)}]" : " (no buttons)";
            LogMessage($"MESSAGE RECEIVED: {message}{buttonInfo}");
        }

        void OnAIMessageReceived(string message)
        {
            lastAIMessage = message;
            
            // Add AI response to chat interface
            AddChatMessage("bot", message);
            
            // Stop waiting indicator
            waitingForAIResponse = false;
            
            RefreshChatHistory();
            LogMessage($"AI RESPONSE: {message}");
        }

        void OnErrorOccurred(string error)
        {
            lastError = error;
            
            // Add error to chat interface
            AddChatMessage("system", $"❌ Error: {error}");
            
            // Stop waiting indicator
            waitingForAIResponse = false;
            
            LogMessage($"ERROR: {error}");
        }

        void OnChatInitialized()
        {
            chatInitialized = true;
            
            RefreshChatHistory();
            
            // Log the chat state for debugging
            if (chatManager != null)
            {
                string stateInfo = chatManager.GetChatStateInfo();
                LogMessage($"Chat state after initialization: {stateInfo}");
            }
            
            // Check if we have conversation history loaded from API
            if (currentHistory.Count > 0)
            {
                // Display all loaded messages in the UI
                foreach (var historyMessage in currentHistory)
                {
                    AddChatMessage(historyMessage.role, historyMessage.message);
                }
                
                AddChatMessage("system", $"✅ Chat initialized successfully! Resumed conversation with {currentHistory.Count} messages from history (loaded via API).");
                LogMessage($"CHAT INITIALIZED - Resumed conversation with {currentHistory.Count} messages loaded from API");
            }
            else
            {
                AddChatMessage("system", "✅ Chat initialized successfully! Starting fresh conversation with day_one message (no existing conversation found via API).");
                LogMessage("CHAT INITIALIZED - Starting new conversation (no existing conversation found via API)");
            }
        }

        void LogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            logMessages.Add($"[{timestamp}] {message}");
            
            // Limit log size
            if (logMessages.Count > 50)
            {
                logMessages.RemoveAt(0);
            }
            
            Debug.Log($"[ChatManagerTestBed] {message}");
        }

        #region AI Integration Methods

        void LoadAIConfiguration()
        {
            // Load saved configuration
            aiTestBedConfig = AITestBedConfig.Load();
            
            // Update inspector fields with loaded values
            enableAIResponses = aiTestBedConfig.enableAI;
            openAIApiKey = aiTestBedConfig.openAIApiKey;
            openAIModel = aiTestBedConfig.openAIModel;
            n8nRAGEndpoint = aiTestBedConfig.n8nRAGEndpoint;
            n8nApiKey = aiTestBedConfig.n8nApiKey;
            enableParallelProcessing = aiTestBedConfig.enableParallelProcessing;
            
            LogMessage("AI configuration loaded from saved settings");
        }

        void SaveAIConfiguration()
        {
            if (aiTestBedConfig == null)
                aiTestBedConfig = new AITestBedConfig();
                
            // Update config with current values
            aiTestBedConfig.enableAI = enableAIResponses;
            aiTestBedConfig.openAIApiKey = openAIApiKey;
            aiTestBedConfig.openAIModel = openAIModel;
            aiTestBedConfig.n8nRAGEndpoint = n8nRAGEndpoint;
            aiTestBedConfig.n8nApiKey = n8nApiKey;
            aiTestBedConfig.enableParallelProcessing = enableParallelProcessing;
            
            // Save to persistent storage
            aiTestBedConfig.Save();
            LogMessage("AI configuration saved");
        }

        void InitializeAIComponents()
        {
            if (!enableAIResponses)
            {
                LogMessage("AI responses disabled");
                return;
            }

            try
            {
                // Use config helper to create configurations
                if (aiTestBedConfig == null)
                    LoadAIConfiguration();

                openAIConfig = aiTestBedConfig.CreateOpenAIConfig();
                ragConfig = aiTestBedConfig.CreateRAGConfig();
                aiConfig = aiTestBedConfig.CreateAIAgentConfig();

                // Initialize AI Agent if we have valid config
                if (aiConfig.IsValid())
                {
                    aiAgent = new AIAgent(aiConfig, this);
                    SubscribeToAIEvents();
                    LogMessage("✓ AI Agent initialized successfully");
                    
                    string ragStatus = aiTestBedConfig.IsValidForRAG() ? "enabled" : "disabled";
                    AddChatMessage("system", $"🤖 AI Agent ready! OpenAI enabled, RAG {ragStatus}.");
                }
                else
                {
                    LogMessage("AI Agent not initialized - missing OpenAI API key");
                    AddChatMessage("system", "⚠️ AI Agent disabled - please configure OpenAI API key in AI Config section");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR initializing AI components: {ex.Message}");
                AddChatMessage("system", $"❌ Failed to initialize AI: {ex.Message}");
            }
        }

        void SubscribeToAIEvents()
        {
            AIAgent.OnAIResponseGenerated += OnAIResponseGenerated;
            AIAgent.OnProfileUpdated += OnProfileUpdated;
            AIAgent.OnProcessingStatusChanged += OnProcessingStatusChanged;
            AIAgent.OnAIErrorOccurred += OnAIErrorOccurred;
            
            OpenAIClient.OnResponseReceived += OnOpenAIResponseReceived;
            OpenAIClient.OnErrorOccurred += OnOpenAIErrorOccurred;
            OpenAIClient.OnTokensUsed += OnTokensUsed;
            
            N8nRAGClient.OnRAGResponseReceived += OnRAGResponseReceived;
            N8nRAGClient.OnRAGErrorOccurred += OnRAGErrorOccurred;
        }

        void UnsubscribeFromAIEvents()
        {
            AIAgent.OnAIResponseGenerated -= OnAIResponseGenerated;
            AIAgent.OnProfileUpdated -= OnProfileUpdated;
            AIAgent.OnProcessingStatusChanged -= OnProcessingStatusChanged;
            AIAgent.OnAIErrorOccurred -= OnAIErrorOccurred;
            
            OpenAIClient.OnResponseReceived -= OnOpenAIResponseReceived;
            OpenAIClient.OnErrorOccurred -= OnOpenAIErrorOccurred;
            OpenAIClient.OnTokensUsed -= OnTokensUsed;
            
            N8nRAGClient.OnRAGResponseReceived -= OnRAGResponseReceived;
            N8nRAGClient.OnRAGErrorOccurred -= OnRAGErrorOccurred;
        }

        void ProcessWithAI(string userMessage)
        {
            if (aiAgent == null)
            {
                AddChatMessage("system", "❌ AI Agent not available");
                waitingForAIResponse = false;
                return;
            }

            // Build conversation history for AI context
            var conversationHistory = new List<string>();
            foreach (var msg in chatUIMessages.Where(m => m.role == "user" || m.role == "bot").TakeLast(10))
            {
                conversationHistory.Add($"{msg.role}: {msg.message}");
            }

            // Create AI request
            var aiRequest = new AIAgent.AIRequest
            {
                userMessage = userMessage,
                conversationHistory = conversationHistory,
                existingProfile = currentUserProfile,
                conversationContext = GetConversationContext(),
                categories = N8nRAGClient.ClassifyQuery(userMessage)
            };

            LogMessage($"Processing with AI: {userMessage}");
            AddChatMessage("system", $"🤖 Processing with AI (Categories: {string.Join(", ", aiRequest.categories)})");

            // Process with AI Agent
            aiAgent.ProcessRequest(aiRequest, OnAIRequestComplete);
        }

        void OnAIRequestComplete(AIAgent.AIResponse response)
        {
            waitingForAIResponse = false;
            currentProcessingStatus = "";

            if (response.success)
            {
                // Add AI response to chat
                AddChatMessage("bot", response.message);
                
                // Update profile if generated
                if (!string.IsNullOrEmpty(response.updatedProfile))
                {
                    currentUserProfile = response.updatedProfile;
                }
                
                // Store processing metrics
                lastTokensUsed = response.tokensUsed;
                lastProcessingTime = response.processingTime;
                
                LogMessage($"AI response generated successfully. Tokens: {response.tokensUsed}, Time: {response.processingTime:F2}s");
            }
            else
            {
                AddChatMessage("system", $"❌ AI processing failed: {response.error}");
                LogMessage($"AI processing failed: {response.error}");
            }
        }

        string GetConversationContext()
        {
            // Provide context about the current conversation for AI processing
            var contextBuilder = new System.Text.StringBuilder();
            
            if (chatManager != null && chatManager.IsInitialized())
            {
                contextBuilder.AppendLine($"Conversation Type: {(chatManager.IsInPredefinedFlow() ? "Predefined Flow" : "Free Conversation")}");
                contextBuilder.AppendLine($"Message Count: {currentHistory.Count}");
            }
            
            contextBuilder.AppendLine($"User Profile Available: {!string.IsNullOrEmpty(currentUserProfile)}");
            
            return contextBuilder.ToString();
        }

        void DrawAIConfigSection()
        {
            GUILayout.Label("=== AI CONFIGURATION ===", GUI.skin.box);
            
            // AI Toggle
            enableAIResponses = GUILayout.Toggle(enableAIResponses, "Enable AI Responses");
            
            if (!enableAIResponses)
            {
                GUILayout.Label("AI responses disabled. Enable to configure OpenAI and RAG.");
                return;
            }
            
            GUILayout.Space(5);
            
            // OpenAI Configuration
            GUILayout.Label("OpenAI Configuration:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("API Key:", GUILayout.Width(80));
            string newApiKey = GUILayout.TextField(openAIApiKey, GUILayout.Width(200));
            if (newApiKey != openAIApiKey)
            {
                openAIApiKey = newApiKey;
                SaveAIConfiguration(); // Auto-save on change
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Model:", GUILayout.Width(80));
            string newModel = GUILayout.TextField(openAIModel, GUILayout.Width(120));
            if (newModel != openAIModel)
            {
                openAIModel = newModel;
                SaveAIConfiguration();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // n8n RAG Configuration
            GUILayout.Label("n8n RAG Configuration:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Endpoint:", GUILayout.Width(80));
            string newEndpoint = GUILayout.TextField(n8nRAGEndpoint, GUILayout.Width(200));
            if (newEndpoint != n8nRAGEndpoint)
            {
                n8nRAGEndpoint = newEndpoint;
                SaveAIConfiguration();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("API Key:", GUILayout.Width(80));
            string newRAGKey = GUILayout.TextField(n8nApiKey, GUILayout.Width(200));
            if (newRAGKey != n8nApiKey)
            {
                n8nApiKey = newRAGKey;
                SaveAIConfiguration();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Processing Options
            GUILayout.Label("Processing Options:", EditorStyles.boldLabel);
            bool newParallel = GUILayout.Toggle(enableParallelProcessing, "Enable Parallel Processing (Profile + RAG)");
            if (newParallel != enableParallelProcessing)
            {
                enableParallelProcessing = newParallel;
                SaveAIConfiguration();
            }
            
            GUILayout.Space(5);
            
            // Action buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Reinitialize AI", GUILayout.Width(130)))
            {
                SaveAIConfiguration(); // Save before reinitializing
                InitializeAIComponents();
            }
            if (GUILayout.Button("💾 Save Config", GUILayout.Width(100)))
            {
                SaveAIConfiguration();
                AddChatMessage("system", "💾 AI configuration saved");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🧠 Show Profile", GUILayout.Width(120)))
            {
                ShowCurrentProfile();
            }
            if (GUILayout.Button("🧪 Test AI", GUILayout.Width(80)))
            {
                TestAIConnection();
            }
            if (GUILayout.Button("📄 Show Config", GUILayout.Width(100)))
            {
                ShowConfigSummary();
            }
            GUILayout.EndHorizontal();
            
            // Status display
            if (aiAgent != null)
            {
                GUI.color = Color.green;
                GUILayout.Label("✓ AI Agent is ready and configured");
                GUI.color = Color.white;
            }
            else if (enableAIResponses)
            {
                GUI.color = Color.red;
                GUILayout.Label("✗ AI Agent failed to initialize - check configuration");
                GUI.color = Color.white;
            }
        }

        void ShowCurrentProfile()
        {
            if (string.IsNullOrEmpty(currentUserProfile))
            {
                AddChatMessage("system", "👤 No user profile available yet. Send some messages to generate a profile.");
            }
            else
            {
                AddChatMessage("system", $"👤 Current User Profile:\n{currentUserProfile}");
            }
        }

        void TestAIConnection()
        {
            if (aiAgent == null)
            {
                AddChatMessage("system", "❌ AI Agent not initialized");
                return;
            }

            AddChatMessage("system", "🧪 Testing AI connection...");
            
            var testRequest = new AIAgent.AIRequest
            {
                userMessage = "Hello, this is a test message to verify AI functionality.",
                conversationHistory = new List<string>(),
                existingProfile = "",
                conversationContext = "Test request from TestBed",
                categories = new string[] { "general" }
            };

            aiAgent.ProcessRequest(testRequest, (response) => {
                if (response.success)
                {
                    AddChatMessage("system", $"✅ AI test successful! Response: {response.message}");
                }
                else
                {
                    AddChatMessage("system", $"❌ AI test failed: {response.error}");
                }
            });
        }

        void ShowConfigSummary()
        {
            if (aiTestBedConfig == null)
            {
                AddChatMessage("system", "⚠️ No configuration loaded");
                return;
            }

            string summary = aiTestBedConfig.GetConfigSummary();
            AddChatMessage("system", $"📄 Configuration Summary:\n{summary}");
        }

        #endregion

        #region AI Event Handlers

        void OnAIResponseGenerated(AIAgent.AIResponse response)
        {
            LogMessage($"AI response generated: {(string.IsNullOrEmpty(response.message) ? "empty" : response.message.Substring(0, Math.Min(50, response.message.Length)) + "...")}");
        }

        void OnProfileUpdated(string profile)
        {
            currentUserProfile = profile;
            LogMessage("User profile updated");
        }

        void OnProcessingStatusChanged(string status)
        {
            currentProcessingStatus = status;
            LogMessage($"AI Status: {status}");
        }

        void OnAIErrorOccurred(string error)
        {
            LogMessage($"AI Error: {error}");
        }

        void OnOpenAIResponseReceived(string response)
        {
            LogMessage($"OpenAI response received: {(string.IsNullOrEmpty(response) ? "empty" : response.Substring(0, Math.Min(30, response.Length)) + "...")}");
        }

        void OnOpenAIErrorOccurred(string error)
        {
            LogMessage($"OpenAI Error: {error}");
        }

        void OnTokensUsed(int tokens)
        {
            lastTokensUsed = tokens;
            LogMessage($"OpenAI tokens used: {tokens}");
        }

        void OnRAGResponseReceived(N8nRAGClient.RAGResponse response)
        {
            if (response != null && response.success)
            {
                LogMessage($"RAG response received: {response.results?.Length ?? 0} results");
            }
        }

        void OnRAGErrorOccurred(string error)
        {
            LogMessage($"RAG Error: {error}");
        }

        #endregion
    }
}