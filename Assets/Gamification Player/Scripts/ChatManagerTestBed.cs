using System;
using System.Collections.Generic;
using UnityEngine;
using GamificationPlayer;
using GamificationPlayer.Session;
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
        
        // Mock dependencies
        private GamificationPlayerEndpoints mockEndpoints;
        private SessionLogData mockSessionData;
        private EnvironmentConfig environmentConfig;
        
        // GUI State
        private string userInputText = "";
        private string buttonInputText = "start-button";
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 logScrollPosition = Vector2.zero;
        private List<string> logMessages = new List<string>();
        private bool showChatHistory = true;
        private bool showLogs = false;
        private bool showControls = false;
        private Rect windowRect;
        
        // Chat UI State
        private List<ChatUIMessage> chatUIMessages = new List<ChatUIMessage>();
        private bool waitingForAIResponse = false;
        
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
            
            // Send to ChatManager
            if (chatManager != null)
            {
                chatManager.HandleUserMessage(message);
                LogMessage($"User sent message: {message}");
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
            ResetChatInterface();
            
            if (chatManager != null && chatManager.IsInitialized())
            {
                // Reset the chat manager state
                chatManager.ResetChat();
                
                // Clear UI messages
                chatUIMessages.Clear();
                
                // Force create new conversation (ignores any existing conversation found via API)
                chatManager.InitializeChat(true);
                
                AddChatMessage("system", "🔄 Forcing new conversation - will create fresh chat session via API (ignoring any existing conversation)");
                LogMessage("Forcing new conversation with forceNewConversation=true (API-based)");
            }
            else
            {
                LogMessage("ChatManager not initialized, cannot force new conversation");
            }
        }
        
        void ResumeExistingConversation()
        {
            ResetChatInterface();
            
            if (chatManager != null && chatManager.IsInitialized())
            {
                // Reset the chat manager state
                chatManager.ResetChat();
                
                // Clear UI messages
                chatUIMessages.Clear();
                
                // Check for existing conversation via API first, create new if none found
                chatManager.InitializeChat(false);
                
                AddChatMessage("system", "📜 Checking for existing conversation via API - will resume if found, create new if not");
                LogMessage("Resuming existing conversation with forceNewConversation=false (API-based detection)");
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
    }
}