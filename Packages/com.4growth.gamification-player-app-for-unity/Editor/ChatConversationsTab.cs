using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// UI implementation for the Conversations tab
    /// Handles displaying, creating, editing, and managing chat conversations per profile
    /// </summary>
    public class ChatConversationsTab
    {
        #region Private Fields
        
        private ChatDataCache dataCache;
        private ChatDatabaseAPI api;
        private Vector2 leftScrollPosition;
        private Vector2 rightScrollPosition;
        private string searchTerm = "";
        
        // Selection state
        private ChatProfileData selectedProfile;
        private ChatConversationData selectedConversation;
        
        // UI state
        private bool showCreateConversationForm = false;
        
        #endregion
        
        #region Constructor
        
        public ChatConversationsTab(ChatDataCache cache, ChatDatabaseAPI databaseApi)
        {
            dataCache = cache;
            api = databaseApi;
        }
        
        #endregion
        
        #region Public Methods
        
        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            
            // Left panel - Profile selection
            DrawProfilesPanel();
            
            // Right panel - Conversations for selected profile
            DrawConversationsPanel();
            
            GUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Private Methods - UI Drawing
        
        private void DrawProfilesPanel()
        {
            GUILayout.BeginVertical(GUILayout.Width(300));
            
            EditorGUILayout.LabelField("Select Profile", EditorStyles.boldLabel);
            
            // Search bar for profiles
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            string newSearchTerm = EditorGUILayout.TextField(searchTerm);
            if (newSearchTerm != searchTerm)
            {
                searchTerm = newSearchTerm;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Profiles list
            var profiles = dataCache.SearchProfiles(searchTerm);
            
            leftScrollPosition = EditorGUILayout.BeginScrollView(leftScrollPosition);
            
            foreach (var profile in profiles)
            {
                DrawProfileItem(profile);
            }
            
            if (profiles.Count == 0)
            {
                EditorGUILayout.HelpBox("No profiles found. Create profiles in the Chat Profiles tab first.", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.EndVertical();
        }
        
        private void DrawProfileItem(ChatProfileData profile)
        {
            bool isSelected = selectedProfile == profile;
            var conversations = dataCache.GetConversationsForProfile(profile.id);
            
            EditorGUILayout.BeginHorizontal(isSelected ? EditorStyles.helpBox : GUIStyle.none);
            
            if (GUILayout.Button("", GUIStyle.none, GUILayout.ExpandWidth(true)))
            {
                selectedProfile = profile;
                selectedConversation = null; // Clear conversation selection when switching profiles
            }
            
            GUILayout.BeginVertical();
            
            // Profile name with status
            string statusIcon = GetStatusIcon(profile.status);
            EditorGUILayout.LabelField($"{statusIcon} {profile.name}", EditorStyles.boldLabel);
            
            // Conversation count
            EditorGUILayout.LabelField($"Conversations: {conversations.Count}", EditorStyles.miniLabel);
            
            GUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            if (isSelected)
            {
                EditorGUILayout.Space();
            }
        }
        
        private void DrawConversationsPanel()
        {
            GUILayout.BeginVertical();
            
            if (selectedProfile == null)
            {
                EditorGUILayout.HelpBox("Select a profile from the left panel to view its conversations", MessageType.Info);
                GUILayout.EndVertical();
                return;
            }
            
            DrawConversationsToolbar();
            DrawConversationsList();
            DrawConversationDetails();
            
            GUILayout.EndVertical();
        }
        
        private void DrawConversationsToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            EditorGUILayout.LabelField($"Conversations for '{selectedProfile.name}'", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Create New Conversation", EditorStyles.toolbarButton))
            {
                CreateNewConversation();
            }
            
            if (selectedConversation != null)
            {
                if (GUILayout.Button("Delete Conversation", EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("Delete Conversation",
                        $"Are you sure you want to delete this conversation and all its messages?", "Delete", "Cancel"))
                    {
                        DeleteConversation();
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawConversationsList()
        {
            var conversations = dataCache.GetConversationsForProfile(selectedProfile.id);
            
            EditorGUILayout.LabelField($"Conversations ({conversations.Count})", EditorStyles.boldLabel);
            
            rightScrollPosition = EditorGUILayout.BeginScrollView(rightScrollPosition, GUILayout.Height(250));
            
            if (conversations.Count == 0)
            {
                EditorGUILayout.HelpBox("No conversations found. Click 'Create New Conversation' to start.", MessageType.Info);
            }
            else
            {
                foreach (var conversation in conversations.OrderByDescending(c => c.lastModified))
                {
                    DrawConversationItem(conversation);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawConversationItem(ChatConversationData conversation)
        {
            bool isSelected = selectedConversation == conversation;
            var messages = dataCache.GetMessagesForConversation(conversation.id);
            
            EditorGUILayout.BeginHorizontal(isSelected ? EditorStyles.helpBox : GUIStyle.none);
            
            if (GUILayout.Button("", GUIStyle.none, GUILayout.ExpandWidth(true)))
            {
                selectedConversation = conversation;
            }
            
            GUILayout.BeginVertical();
            
            // Conversation header with status
            string statusIcon = GetStatusIcon(conversation.status);
            string conversationTitle = $"Conversation {conversation.id.Substring(0, 8)}...";
            EditorGUILayout.LabelField($"{statusIcon} {conversationTitle}", EditorStyles.boldLabel);
            
            // Message count and dates
            EditorGUILayout.LabelField($"Messages: {messages.Count}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Created: {conversation.createdAt:MM/dd/yyyy HH:mm}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Modified: {conversation.lastModified:MM/dd/yyyy HH:mm}", EditorStyles.miniLabel);
            
            // Show last message preview if available
            if (messages.Count > 0)
            {
                var lastMessage = messages.OrderBy(m => m.createdAt).LastOrDefault();
                if (lastMessage != null)
                {
                    string preview = lastMessage.content.Length > 50 
                        ? lastMessage.content.Substring(0, 50) + "..." 
                        : lastMessage.content;
                    EditorGUILayout.LabelField($"Last: [{lastMessage.role}] {preview}", EditorStyles.miniLabel);
                }
            }
            
            GUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            if (isSelected)
            {
                EditorGUILayout.Space();
            }
        }
        
        private void DrawConversationDetails()
        {
            if (selectedConversation == null)
            {
                if (selectedProfile != null)
                {
                    EditorGUILayout.HelpBox("Select a conversation to view details and messages", MessageType.Info);
                }
                return;
            }
            
            EditorGUILayout.LabelField("Conversation Details", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("ID:", selectedConversation.id);
            EditorGUILayout.LabelField("Profile:", selectedProfile.name);
            EditorGUILayout.LabelField("Status:", selectedConversation.status);
            EditorGUILayout.LabelField("Created:", selectedConversation.createdAt.ToString("yyyy-MM-dd HH:mm:ss"));
            EditorGUILayout.LabelField("Modified:", selectedConversation.lastModified.ToString("yyyy-MM-dd HH:mm:ss"));
            
            EditorGUILayout.EndVertical();
            
            // Show messages preview
            var messages = dataCache.GetMessagesForConversation(selectedConversation.id);
            EditorGUILayout.LabelField($"Messages ({messages.Count})", EditorStyles.boldLabel);
            
            if (messages.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                foreach (var message in messages.OrderBy(m => m.createdAt).Take(5)) // Show first 5 messages
                {
                    string roleColor = message.role == "user" ? "blue" : "green";
                    string preview = message.content.Length > 100 
                        ? message.content.Substring(0, 100) + "..." 
                        : message.content;
                    
                    EditorGUILayout.LabelField($"[{message.role.ToUpper()}] {preview}");
                    EditorGUILayout.LabelField($"  {message.createdAt:HH:mm:ss}", EditorStyles.miniLabel);
                    EditorGUILayout.Space();
                }
                
                if (messages.Count > 5)
                {
                    EditorGUILayout.LabelField($"... and {messages.Count - 5} more messages");
                }
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("View All Messages in Messages Tab"))
                {
                    // This could trigger a switch to the Messages tab with this conversation selected
                    Debug.Log($"Switch to Messages tab for conversation {selectedConversation.id}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No messages in this conversation yet. Add messages in the Messages tab.", MessageType.Info);
            }
        }
        
        #endregion
        
        #region Private Methods - Actions
        
        private void CreateNewConversation()
        {
            if (selectedProfile == null) return;
            
            var newConversation = new ChatConversationData(selectedProfile.id);
            
            api.CreateConversation(newConversation, (success, error) =>
            {
                if (success)
                {
                    selectedConversation = newConversation;
                    Debug.Log($"New conversation created for profile '{selectedProfile.name}'");
                }
                else
                {
                    Debug.LogError($"Failed to create conversation: {error}");
                }
            });
        }
        
        private void DeleteConversation()
        {
            if (selectedConversation == null) return;
            
            string conversationId = selectedConversation.id;
            
            api.DeleteConversation(conversationId, (success, error) =>
            {
                if (success)
                {
                    selectedConversation = null;
                    Debug.Log($"Conversation deleted successfully");
                }
                else
                {
                    Debug.LogError($"Failed to delete conversation: {error}");
                }
            });
        }
        
        #endregion
        
        #region Private Methods - Utilities
        
        private string GetStatusIcon(string status)
        {
            switch (status)
            {
                case "synced": return "✓";
                case "modified": return "●";
                case "new": return "+";
                case "error": return "⚠";
                default: return "○";
            }
        }
        
        #endregion
        
        #region Public Methods - External Interface
        
        /// <summary>
        /// Set the selected profile from external tabs
        /// </summary>
        public void SetSelectedProfile(ChatProfileData profile)
        {
            selectedProfile = profile;
            selectedConversation = null;
        }
        
        /// <summary>
        /// Get the currently selected conversation for use by other tabs
        /// </summary>
        public ChatConversationData GetSelectedConversation()
        {
            return selectedConversation;
        }
        
        /// <summary>
        /// Set the selected conversation from external tabs  
        /// </summary>
        public void SetSelectedConversation(ChatConversationData conversation)
        {
            selectedConversation = conversation;
            if (conversation != null)
            {
                // Also set the profile
                selectedProfile = dataCache.GetProfile(conversation.profileId);
            }
        }
        
        #endregion
    }
}