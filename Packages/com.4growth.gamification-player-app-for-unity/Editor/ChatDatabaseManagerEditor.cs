using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// Unity Editor window for managing chat database through API endpoints
    /// Provides CRUD operations for all chat-related data with real-time synchronization
    /// </summary>
    public class ChatDatabaseManagerEditor : EditorWindow
    {
        #region Private Fields
        
        // Core Components
        private ChatDatabaseAPI apiClient;
        private ChatDataCache dataCache;
        private EnvironmentConfig environmentConfig;
        
        // Tab Implementations
        private ChatProfilesTab profilesTab;
        private ChatConversationsTab conversationsTab;
        private ChatMessagesTab messagesTab;
        private ChatPredefinedMessagesTab predefinedMessagesTab;
        private ChatInstructionsTab instructionsTab;
        private ChatSyncUtilsTab syncUtilsTab;
        
        // UI State
        private int selectedTab = 0;
        private Vector2 scrollPosition;
        private string searchQuery = "";
        private bool showConnectionSettings = true;
        
        // Connection State
        private bool isConnected = false;
        private string connectionStatus = "Disconnected";
        private DateTime lastSyncTime;
        
        // API Context IDs (required for all API calls in Editor context)
        private string organizationId = "";
        private string microGameId = "";
        
        // Tab Names
        private readonly string[] tabNames = {
            "Chat Profiles",
            "Conversations", 
            "Messages",
            "Predefined Messages",
            "Instructions",
            "Sync & Utils"
        };
        
        // Colors for data states
        private readonly Color syncedColor = Color.green;
        private readonly Color modifiedColor = Color.yellow;
        private readonly Color errorColor = Color.red;
        private readonly Color newColor = Color.blue;
        
        // GUI Styles (initialized in OnEnable)
        private GUIStyle headerStyle;
        private GUIStyle tabStyle;
        private GUIStyle connectionStatusStyle;
        
        #endregion
        
        #region Unity Editor Menu
        
        [MenuItem("Gamification Player/Chat Database Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<ChatDatabaseManagerEditor>("Chat Database Manager");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        

        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnEnable()
        {
            LoadWindowState();
            InitializeStyles();
            LoadEnvironmentConfig();
        }
        
        private void OnDisable()
        {
            SaveWindowState();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawConnectionPanel();
            DrawTabNavigation();
            DrawMainContent();
            DrawFooter();
            
            // Handle keyboard shortcuts
            HandleKeyboardInput();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeStyles()
        {
            // Header style
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            // Tab style
            tabStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                fixedHeight = 25
            };
            
            // Connection status style
            connectionStatusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
        }
        
        private void InitializeComponents()
        {
            if (dataCache == null)
                dataCache = new ChatDataCache();
                
            if (apiClient == null && dataCache != null)
            {
                apiClient = new ChatDatabaseAPI(dataCache, environmentConfig);
                UpdateApiContextIds(); // Set organization and microgame IDs for Editor context
            }
                
            // Initialize tab implementations
            if (profilesTab == null && dataCache != null && apiClient != null)
                profilesTab = new ChatProfilesTab(dataCache, apiClient);
            if (conversationsTab == null && dataCache != null && apiClient != null)
                conversationsTab = new ChatConversationsTab(dataCache, apiClient);
            if (messagesTab == null && dataCache != null && apiClient != null)
            {
                messagesTab = new ChatMessagesTab();
                messagesTab.Initialize(apiClient, dataCache);
            }
            if (predefinedMessagesTab == null && dataCache != null && apiClient != null)
            {
                predefinedMessagesTab = new ChatPredefinedMessagesTab();
                predefinedMessagesTab.Initialize(apiClient, dataCache);
            }
            if (instructionsTab == null && dataCache != null && apiClient != null)
            {
                instructionsTab = new ChatInstructionsTab();
                instructionsTab.Initialize(apiClient, dataCache);
            }
            if (syncUtilsTab == null && dataCache != null && apiClient != null)
            {
                syncUtilsTab = new ChatSyncUtilsTab();
                syncUtilsTab.Initialize(apiClient, dataCache);
            }
        }
        
        private void LoadEnvironmentConfig()
        {
            // Try to find existing EnvironmentConfig asset
            var configs = AssetDatabase.FindAssets("t:EnvironmentConfig");
            if (configs.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(configs[0]);
                environmentConfig = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(path);
                InitializeComponents();
                CheckConnection();
            }
        }
        
        private void UpdateApiContextIds()
        {
            if (apiClient != null)
            {
                apiClient.SetContextIds(organizationId, microGameId);
            }
        }
        
        #endregion
        
        #region UI Drawing Methods
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Title
            GUILayout.Label("Chat Database Manager", headerStyle);
            
            GUILayout.FlexibleSpace();
            
            // Global search
            EditorGUI.BeginChangeCheck();
            searchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck())
            {
                OnSearchQueryChanged();
            }
            
            // Refresh button
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshAllData();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawConnectionPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Connection foldout
            showConnectionSettings = EditorGUILayout.Foldout(showConnectionSettings, "Connection Settings", true);
            
            if (showConnectionSettings)
            {
                EditorGUI.indentLevel++;
                
                // Environment Config
                EditorGUI.BeginChangeCheck();
                environmentConfig = (EnvironmentConfig)EditorGUILayout.ObjectField(
                    "Environment Config", environmentConfig, typeof(EnvironmentConfig), false);
                if (EditorGUI.EndChangeCheck())
                {
                    InitializeComponents();
                }
                
                // API Context IDs (required for Editor API calls)
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("API Context IDs", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                organizationId = EditorGUILayout.TextField("Organization ID", organizationId);
                microGameId = EditorGUILayout.TextField("Microgame ID", microGameId);
                if (EditorGUI.EndChangeCheck())
                {
                    // Update API client with new IDs when they change
                    UpdateApiContextIds();
                }
                
                // Validation for required IDs
                if (string.IsNullOrEmpty(organizationId) || string.IsNullOrEmpty(microGameId))
                {
                    EditorGUILayout.HelpBox(
                        "Organization ID and Microgame ID are required for API calls in Editor context.", 
                        MessageType.Warning);
                }
                
                if (environmentConfig != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Connection status
                    var statusColor = isConnected ? syncedColor : errorColor;
                    var prevColor = GUI.color;
                    GUI.color = statusColor;
                    GUILayout.Label($"● {connectionStatus}", connectionStatusStyle);
                    GUI.color = prevColor;
                    
                    GUILayout.FlexibleSpace();
                    
                    // API URL (read-only)
                    EditorGUILayout.LabelField("API URL:", environmentConfig.API_URL, EditorStyles.miniLabel);
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // Connection actions
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button("Test Connection", GUILayout.Width(120)))
                    {
                        TestConnection();
                    }
                    
                    if (GUILayout.Button("Reconnect", GUILayout.Width(80)))
                    {
                        CheckConnection();
                    }
                    
                    GUILayout.FlexibleSpace();
                    
                    // Last sync time
                    if (lastSyncTime != default)
                    {
                        EditorGUILayout.LabelField($"Last Sync: {lastSyncTime:HH:mm:ss}", 
                            EditorStyles.miniLabel, GUILayout.Width(100));
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Please assign an EnvironmentConfig asset to connect to the chat database.", 
                        MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTabNavigation()
        {
            EditorGUILayout.BeginHorizontal();
            
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames, tabStyle);
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);
        }
        
        private void DrawMainContent()
        {
            if (environmentConfig == null || !isConnected)
            {
                DrawNotConnectedMessage();
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTab)
            {
                case 0: 
                    if (profilesTab != null) 
                        profilesTab.OnGUI(); 
                    else 
                        DrawChatProfilesTab(); 
                    break;
                case 1: 
                    if (conversationsTab != null) 
                        conversationsTab.OnGUI(); 
                    else 
                        DrawConversationsTab(); 
                    break;
                case 2: 
                    if (messagesTab != null) 
                        messagesTab.DrawTab(); 
                    else 
                        DrawMessagesTab(); 
                    break;
                case 3: 
                    if (predefinedMessagesTab != null) 
                        predefinedMessagesTab.DrawTab(); 
                    else 
                        DrawPredefinedMessagesTab(); 
                    break;
                case 4: 
                    if (instructionsTab != null) 
                        instructionsTab.DrawTab(); 
                    else 
                        DrawInstructionsTab(); 
                    break;
                case 5: 
                    if (syncUtilsTab != null) 
                        syncUtilsTab.DrawTab(); 
                    else 
                        DrawSyncUtilsTab(); 
                    break;
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Status messages
            if (dataCache != null)
            {
                var stats = dataCache.GetStatistics();
                GUILayout.Label($"Profiles: {stats.profileCount} | " +
                              $"Conversations: {stats.conversationCount} | " +
                              $"Messages: {stats.messageCount} | " +
                              $"Unsaved: {stats.unsavedCount}", 
                              EditorStyles.miniLabel);
            }
            
            GUILayout.FlexibleSpace();
            
            // Action buttons
            if (dataCache != null && dataCache.HasUnsavedChanges())
            {
                if (GUILayout.Button("Save All", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    SaveAllChanges();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Tab Content Methods
        
        private void DrawNotConnectedMessage()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            
            EditorGUILayout.HelpBox(
                "Not connected to chat database.\n\n" +
                "Please ensure:\n" +
                "• EnvironmentConfig is assigned\n" +
                "• API connection is working\n" +
                "• Authentication is valid", 
                MessageType.Info);
            
            if (GUILayout.Button("Retry Connection"))
            {
                CheckConnection();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawChatProfilesTab()
        {
            EditorGUILayout.LabelField("Chat Profiles", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Chat Profiles tab is not properly initialized. Please check the connection and try refreshing.", MessageType.Warning);
            
            if (GUILayout.Button("Load Profiles"))
            {
                LoadChatProfiles();
            }
        }
        
        private void DrawConversationsTab()
        {
            EditorGUILayout.LabelField("Chat Conversations", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Conversations tab is not properly initialized. Please check the connection and try refreshing.", MessageType.Warning);
        }
        
        private void DrawMessagesTab()
        {
            EditorGUILayout.LabelField("Chat Messages", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Messages tab is not properly initialized. Please check the connection and try refreshing.", MessageType.Warning);
        }
        
        private void DrawPredefinedMessagesTab()
        {
            EditorGUILayout.LabelField("Predefined Messages", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Predefined Messages tab is not properly initialized. Please check the connection and try refreshing.", MessageType.Warning);
        }
        
        private void DrawInstructionsTab()
        {
            EditorGUILayout.LabelField("Chat Instructions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Instructions tab is not properly initialized. Please check the connection and try refreshing.", MessageType.Warning);
        }
        
        private void DrawSyncUtilsTab()
        {
            EditorGUILayout.LabelField("Sync & Utilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Sync & Utils tab is not properly initialized. Please check the connection and try refreshing.", MessageType.Warning);
        }
        
        #endregion
        
        #region Connection Management
        
        private void CheckConnection()
        {
            if (environmentConfig == null)
            {
                isConnected = false;
                connectionStatus = "No Config";
                return;
            }

            // Test connection by attempting to initialize API client
            try
            {
                if (apiClient != null && dataCache != null)
                {
                    isConnected = true;
                    connectionStatus = "Connected";
                    lastSyncTime = DateTime.Now;
                }
                else
                {
                    isConnected = false;
                    connectionStatus = "Init Failed";
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                connectionStatus = $"Error: {ex.Message}";
                Debug.LogError($"Connection check failed: {ex.Message}");
            }
        }        private void TestConnection()
        {
            if (environmentConfig == null)
            {
                EditorUtility.DisplayDialog("Connection Test", "No EnvironmentConfig assigned.", "OK");
                return;
            }

            connectionStatus = "Testing...";
            Repaint();

            try
            {
                // Test API client initialization and basic functionality
                InitializeComponents();
                
                if (apiClient != null && dataCache != null)
                {
                    // Connection test successful
                    isConnected = true;
                    connectionStatus = "Connected";
                    lastSyncTime = DateTime.Now;
                    EditorUtility.DisplayDialog("Connection Test", 
                        $"Connection successful!\nAPI URL: {environmentConfig.API_URL}", "OK");
                }
                else
                {
                    isConnected = false;
                    connectionStatus = "Failed";
                    EditorUtility.DisplayDialog("Connection Test", "Failed to initialize API client.", "OK");
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                connectionStatus = "Error";
                EditorUtility.DisplayDialog("Connection Test", 
                    $"Connection failed:\n{ex.Message}", "OK");
                Debug.LogError($"Connection test failed: {ex}");
            }
            
            Repaint();
        }
        
        #endregion
        
        #region Data Operations
        
        private void OnSearchQueryChanged()
        {
            // Search query is stored and can be used by individual tabs
            // Each tab implementation has its own internal search functionality
            // Force repaint to allow tabs to update their filtered views
            Repaint();
        }
        
        private void RefreshAllData()
        {
            if (!isConnected || apiClient == null || dataCache == null)
            {
                EditorUtility.DisplayDialog("Refresh Failed", "Not connected to database.", "OK");
                return;
            }

            try
            {
                // Clear cache to force fresh data loading
                dataCache.ClearAll();
                
                lastSyncTime = DateTime.Now;
                Debug.Log("Data cache cleared - fresh data will be loaded on next access");
                
                // Force UI repaint to trigger data reload in tabs
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to refresh data: {ex.Message}");
                EditorUtility.DisplayDialog("Refresh Failed", 
                    $"Failed to refresh data:\n{ex.Message}", "OK");
            }
        }
        
        private void LoadChatProfiles()
        {
            if (!isConnected || apiClient == null || dataCache == null)
            {
                EditorUtility.DisplayDialog("Load Failed", "Not connected to database.", "OK");
                return;
            }

            try
            {
                // Load profiles through the API client
                apiClient.GetAllProfiles((profiles, error) =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.Log($"Loaded {profiles.Count} chat profiles");
                        Repaint();
                    }
                    else
                    {
                        Debug.LogError($"Failed to load profiles: {error}");
                        EditorUtility.DisplayDialog("Load Failed", $"Failed to load profiles:\n{error}", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load profiles: {ex.Message}");
                EditorUtility.DisplayDialog("Load Failed", 
                    $"Failed to load profiles:\n{ex.Message}", "OK");
            }
        }
        
        private void SaveAllChanges()
        {
            if (!isConnected || apiClient == null || dataCache == null)
            {
                EditorUtility.DisplayDialog("Save Failed", "Not connected to database.", "OK");
                return;
            }

            try
            {
                // Note: Individual tabs handle their own save operations
                // This is a placeholder for coordinated save functionality
                Debug.Log("Save All Changes requested - individual tabs handle their own save operations");
                EditorUtility.DisplayDialog("Save Request", 
                    "Save request sent to all tabs.\nIndividual tabs will handle their own save operations.", "OK");
                
                // Force UI repaint
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process save request: {ex.Message}");
                EditorUtility.DisplayDialog("Save Failed", 
                    $"Failed to process save request:\n{ex.Message}", "OK");
            }
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandleKeyboardInput()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.F5)
                {
                    RefreshAllData();
                    e.Use();
                }
                else if (e.control && e.keyCode == KeyCode.S)
                {
                    SaveAllChanges();
                    e.Use();
                }
                else if (e.control && e.keyCode == KeyCode.F)
                {
                    // Focus search field
                    GUI.FocusControl("SearchField");
                    e.Use();
                }
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void SaveWindowState()
        {
            EditorPrefs.SetInt("ChatDBEditor_SelectedTab", selectedTab);
            EditorPrefs.SetBool("ChatDBEditor_ShowConnection", showConnectionSettings);
            EditorPrefs.SetString("ChatDBEditor_OrganizationId", organizationId);
            EditorPrefs.SetString("ChatDBEditor_MicroGameId", microGameId);
        }
        
        private void LoadWindowState()
        {
            selectedTab = EditorPrefs.GetInt("ChatDBEditor_SelectedTab", 0);
            showConnectionSettings = EditorPrefs.GetBool("ChatDBEditor_ShowConnection", true);
            organizationId = EditorPrefs.GetString("ChatDBEditor_OrganizationId", "");
            microGameId = EditorPrefs.GetString("ChatDBEditor_MicroGameId", "");
        }
        
        #endregion
    }
}