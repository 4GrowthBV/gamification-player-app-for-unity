using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// Messages tab for the Chat Database Manager
    /// Provides UI for managing individual chat messages within conversations
    /// </summary>
    public class ChatMessagesTab
    {
        #region Private Fields
        
        private ChatDatabaseAPI api;
        private ChatDataCache dataCache;
        private Vector2 conversationScrollPosition;
        private Vector2 messageScrollPosition;
        private string selectedConversationId = "";
        private string selectedProfileId = "";
        private List<ChatConversationData> availableConversations = new List<ChatConversationData>();
        private List<ChatMessageData> currentMessages = new List<ChatMessageData>();
        private bool isLoadingMessages = false;
        private string searchFilter = "";
        
        // Message creation
        private bool showCreateMessageForm = false;
        private string newMessageRole = "user";
        private string newMessageContent = "";
        private readonly string[] messageRoles = { "user", "assistant", "system" };
        
        // Message editing
        private string editingMessageId = "";
        private string editingMessageContent = "";
        private string editingMessageRole = "";
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the messages tab
        /// </summary>
        public void Initialize(ChatDatabaseAPI api, ChatDataCache dataCache)
        {
            this.api = api;
            this.dataCache = dataCache;
            RefreshConversations();
        }
        
        /// <summary>
        /// Draw the messages tab UI
        /// </summary>
        public void DrawTab()
        {
            EditorGUILayout.BeginVertical();
            
            DrawHeader();
            DrawConversationSelection();
            
            if (!string.IsNullOrEmpty(selectedConversationId))
            {
                DrawMessageManagement();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a conversation to view and manage messages.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Set search query for filtering messages
        /// </summary>
        public void SetSearchQuery(string query)
        {
            if (searchFilter != query)
            {
                searchFilter = query;
                FilterMessages();
            }
        }
        
        #endregion
        
        #region Private Methods - UI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Chat Messages Management", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                RefreshConversations();
                if (!string.IsNullOrEmpty(selectedConversationId))
                {
                    RefreshMessages();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        private void DrawConversationSelection()
        {
            EditorGUILayout.LabelField("Select Conversation", EditorStyles.boldLabel);
            
            // Profile filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter by Profile:", GUILayout.Width(120));
            
            var profiles = dataCache.GetAllProfiles();
            var profileNames = new List<string> { "All Profiles" };
            profileNames.AddRange(profiles.Select(p => $"{p.name} ({p.id.Substring(0, 8)})"));
            
            int selectedProfileIndex = 0;
            if (!string.IsNullOrEmpty(selectedProfileId))
            {
                selectedProfileIndex = profiles.FindIndex(p => p.id == selectedProfileId) + 1;
            }
            
            int newProfileIndex = EditorGUILayout.Popup(selectedProfileIndex, profileNames.ToArray());
            if (newProfileIndex != selectedProfileIndex)
            {
                selectedProfileId = newProfileIndex == 0 ? "" : profiles[newProfileIndex - 1].id;
                RefreshConversations();
                selectedConversationId = "";
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            // Conversation list
            if (availableConversations.Count == 0)
            {
                EditorGUILayout.HelpBox("No conversations found. Create conversations in the Conversations tab.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Conversations ({availableConversations.Count})", EditorStyles.boldLabel);
            
            conversationScrollPosition = EditorGUILayout.BeginScrollView(conversationScrollPosition, GUILayout.Height(150));
            
            foreach (var conversation in availableConversations)
            {
                DrawConversationItem(conversation);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
        }
        
        private void DrawConversationItem(ChatConversationData conversation)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            bool isSelected = conversation.id == selectedConversationId;
            Color originalColor = GUI.backgroundColor;
            
            if (isSelected)
            {
                GUI.backgroundColor = Color.cyan;
            }
            
            if (GUILayout.Button("", GUILayout.ExpandWidth(true)))
            {
                selectedConversationId = conversation.id;
                RefreshMessages();
            }
            
            GUI.backgroundColor = originalColor;
            
            // Overlay conversation info on the button
            Rect lastRect = GUILayoutUtility.GetLastRect();
            
            EditorGUI.LabelField(new Rect(lastRect.x + 5, lastRect.y + 2, lastRect.width - 10, 16), 
                $"ID: {conversation.id.Substring(0, 8)}...", EditorStyles.miniLabel);
                
            EditorGUI.LabelField(new Rect(lastRect.x + 5, lastRect.y + 16, lastRect.width - 10, 16), 
                $"Profile: {conversation.profileId.Substring(0, 8)}...", EditorStyles.miniLabel);
                
            EditorGUI.LabelField(new Rect(lastRect.x + 5, lastRect.y + 30, lastRect.width - 10, 16), 
                $"Created: {conversation.createdAt:MM/dd HH:mm}", EditorStyles.miniLabel);
            
            var messageCount = dataCache.GetMessagesForConversation(conversation.id).Count;
            EditorGUI.LabelField(new Rect(lastRect.x + lastRect.width - 80, lastRect.y + 2, 75, 16), 
                $"{messageCount} msgs", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawMessageManagement()
        {
            EditorGUILayout.LabelField($"Messages in Conversation", EditorStyles.boldLabel);
            
            // Message controls
            EditorGUILayout.BeginHorizontal();
            
            // Search filter
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            string newSearchFilter = EditorGUILayout.TextField(searchFilter, GUILayout.Width(200));
            if (newSearchFilter != searchFilter)
            {
                searchFilter = newSearchFilter;
                FilterMessages();
            }
            
            GUILayout.FlexibleSpace();
            
            // Create message button
            if (GUILayout.Button("Create Message", GUILayout.Width(120)))
            {
                showCreateMessageForm = !showCreateMessageForm;
                if (showCreateMessageForm)
                {
                    newMessageContent = "";
                    newMessageRole = "user";
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Create message form
            if (showCreateMessageForm)
            {
                DrawCreateMessageForm();
            }
            
            EditorGUILayout.Space();
            
            // Messages list
            if (isLoadingMessages)
            {
                EditorGUILayout.LabelField("Loading messages...", EditorStyles.centeredGreyMiniLabel);
            }
            else if (currentMessages.Count == 0)
            {
                EditorGUILayout.HelpBox("No messages in this conversation.", MessageType.Info);
            }
            else
            {
                DrawMessagesList();
            }
        }
        
        private void DrawCreateMessageForm()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Create New Message", EditorStyles.boldLabel);
            
            // Role selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Role:", GUILayout.Width(60));
            int roleIndex = Array.IndexOf(messageRoles, newMessageRole);
            roleIndex = EditorGUILayout.Popup(roleIndex, messageRoles, GUILayout.Width(100));
            newMessageRole = messageRoles[roleIndex];
            EditorGUILayout.EndHorizontal();
            
            // Message content
            EditorGUILayout.LabelField("Content:");
            newMessageContent = DrawConstrainedTextArea(newMessageContent, 80);
            
            // Buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create", GUILayout.Width(80)))
            {
                CreateMessage();
            }
            
            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                showCreateMessageForm = false;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        private void DrawMessagesList()
        {
            EditorGUILayout.LabelField($"Messages ({currentMessages.Count})", EditorStyles.boldLabel);
            
            messageScrollPosition = EditorGUILayout.BeginScrollView(messageScrollPosition, GUILayout.MinHeight(300));
            
            var sortedMessages = currentMessages.OrderBy(m => m.createdAt).ToList();
            
            foreach (var message in sortedMessages)
            {
                DrawMessageItem(message);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawMessageItem(ChatMessageData message)
        {
            bool isEditing = editingMessageId == message.id;
            
            EditorGUILayout.BeginVertical("box");
            
            // Message header
            EditorGUILayout.BeginHorizontal();
            
            // Role indicator
            Color roleColor = message.role == "user" ? Color.green : 
                             message.role == "assistant" ? Color.blue : Color.yellow;
            Color originalColor = GUI.color;
            GUI.color = roleColor;
            EditorGUILayout.LabelField($"[{message.role.ToUpper()}]", EditorStyles.boldLabel, GUILayout.Width(80));
            GUI.color = originalColor;
            
            // Timestamp and ID
            EditorGUILayout.LabelField($"{message.createdAt:HH:mm:ss} | ID: {message.id.Substring(0, 8)}...", 
                EditorStyles.miniLabel);
            
            GUILayout.FlexibleSpace();
            
            // Status indicator
            string statusText = message.status == "synced" ? "✓" : 
                               message.status == "local" ? "⚠" : "○";
            EditorGUILayout.LabelField(statusText, GUILayout.Width(20));
            
            // Action buttons
            if (!isEditing)
            {
                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                {
                    StartEditingMessage(message);
                }
                
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    DeleteMessage(message.id);
                }
            }
            else
            {
                if (GUILayout.Button("Save", GUILayout.Width(50)))
                {
                    SaveEditingMessage();
                }
                
                if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                {
                    CancelEditingMessage();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Message content
            if (isEditing)
            {
                // Edit mode
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Role:", GUILayout.Width(40));
                int roleIndex = Array.IndexOf(messageRoles, editingMessageRole);
                roleIndex = EditorGUILayout.Popup(roleIndex, messageRoles, GUILayout.Width(80));
                editingMessageRole = messageRoles[roleIndex];
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField("Content:");
                editingMessageContent = DrawConstrainedTextArea(editingMessageContent, 60);
            }
            else
            {
                // Display mode
                EditorGUILayout.LabelField("Content:", EditorStyles.miniLabel);
                DrawConstrainedSelectableLabel(message.content, 60, 100);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        /// <summary>
        /// Creates a text area with proper word wrapping, width constraints, and automatic scrolling
        /// </summary>
        // Static field to maintain scroll positions for text areas
        private static Dictionary<string, Vector2> textAreaScrollPositions = new Dictionary<string, Vector2>();
        
        private string DrawConstrainedTextArea(string text, float height, float maxWidthOffset = 50f)
        {
            // Create a unique key for this text area based on calling context
            string scrollKey = $"message_textarea_{text?.GetHashCode() ?? 0}";
            
            if (!textAreaScrollPositions.ContainsKey(scrollKey))
                textAreaScrollPositions[scrollKey] = Vector2.zero;
            
            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            
            // Create an explicit scroll view
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - maxWidthOffset));
            
            textAreaScrollPositions[scrollKey] = EditorGUILayout.BeginScrollView(
                textAreaScrollPositions[scrollKey], 
                GUILayout.Height(height),
                GUILayout.ExpandWidth(true));
            
            string result = EditorGUILayout.TextArea(text, textAreaStyle, GUILayout.ExpandHeight(true));
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            return result;
        }
        
        /// <summary>
        /// Creates a selectable label with proper word wrapping and width constraints
        /// </summary>
        private void DrawConstrainedSelectableLabel(string text, float height, float maxWidthOffset = 50f)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            
            EditorGUILayout.SelectableLabel(text, labelStyle, 
                GUILayout.Height(height),
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - maxWidthOffset));
        }
        
        #endregion
        
        #region Private Methods - Data Operations
        
        private void RefreshConversations()
        {
            availableConversations.Clear();
            
            if (string.IsNullOrEmpty(selectedProfileId))
            {
                // Get all conversations
                var allProfiles = dataCache.GetAllProfiles();
                foreach (var profile in allProfiles)
                {
                    availableConversations.AddRange(dataCache.GetConversationsForProfile(profile.id));
                }
            }
            else
            {
                // Get conversations for selected profile
                availableConversations = dataCache.GetConversationsForProfile(selectedProfileId);
            }
            
            availableConversations = availableConversations.OrderByDescending(c => c.createdAt).ToList();
        }
        
        private void RefreshMessages()
        {
            if (string.IsNullOrEmpty(selectedConversationId))
            {
                currentMessages.Clear();
                return;
            }
            
            isLoadingMessages = true;
            
            api.GetMessagesForConversation(selectedConversationId, (messages, error) =>
            {
                isLoadingMessages = false;
                
                if (error != null)
                {
                    Debug.LogError($"Failed to load messages: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to load messages: {error}", "OK");
                    return;
                }
                
                currentMessages = messages ?? new List<ChatMessageData>();
                FilterMessages();
            });
        }
        
        private void FilterMessages()
        {
            if (string.IsNullOrEmpty(searchFilter))
            {
                // Show all messages from cache
                currentMessages = dataCache.GetMessagesForConversation(selectedConversationId);
            }
            else
            {
                // Filter messages by content
                var allMessages = dataCache.GetMessagesForConversation(selectedConversationId);
                currentMessages = allMessages.Where(m => 
                    m.content.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    m.role.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }
        }
        
        private void CreateMessage()
        {
            if (string.IsNullOrEmpty(newMessageContent.Trim()))
            {
                EditorUtility.DisplayDialog("Invalid Input", "Message content cannot be empty.", "OK");
                return;
            }
            
            var message = new ChatMessageData
            {
                id = Guid.NewGuid().ToString(),
                conversationId = selectedConversationId,
                role = newMessageRole,
                content = newMessageContent.Trim(),
                createdAt = DateTime.UtcNow,
                lastModified = DateTime.UtcNow,
                status = "new"
            };
            
            api.CreateMessage(message, (success, error) =>
            {
                if (success)
                {
                    Debug.Log("Message created successfully");
                    showCreateMessageForm = false;
                    RefreshMessages();
                }
                else
                {
                    Debug.LogError($"Failed to create message: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to create message: {error}", "OK");
                }
            });
        }
        
        private void StartEditingMessage(ChatMessageData message)
        {
            editingMessageId = message.id;
            editingMessageContent = message.content;
            editingMessageRole = message.role;
        }
        
        private void SaveEditingMessage()
        {
            if (string.IsNullOrEmpty(editingMessageContent.Trim()))
            {
                EditorUtility.DisplayDialog("Invalid Input", "Message content cannot be empty.", "OK");
                return;
            }
            
            var message = currentMessages.FirstOrDefault(m => m.id == editingMessageId);
            if (message != null)
            {
                message.content = editingMessageContent.Trim();
                message.role = editingMessageRole;
                message.lastModified = DateTime.UtcNow;
                message.status = "modified";
                
                api.UpdateMessage(message, (success, error) =>
                {
                    if (success)
                    {
                        Debug.Log("Message updated successfully");
                        CancelEditingMessage();
                        RefreshMessages();
                    }
                    else
                    {
                        Debug.LogError($"Failed to update message: {error}");
                        EditorUtility.DisplayDialog("Error", $"Failed to update message: {error}", "OK");
                    }
                });
            }
        }
        
        private void CancelEditingMessage()
        {
            editingMessageId = "";
            editingMessageContent = "";
            editingMessageRole = "";
        }
        
        private void DeleteMessage(string messageId)
        {
            if (EditorUtility.DisplayDialog("Delete Message", 
                "Are you sure you want to delete this message? This action cannot be undone.", 
                "Delete", "Cancel"))
            {
                api.DeleteMessage(messageId, (success, error) =>
                {
                    if (success)
                    {
                        Debug.Log("Message deleted successfully");
                        RefreshMessages();
                    }
                    else
                    {
                        Debug.LogError($"Failed to delete message: {error}");
                        EditorUtility.DisplayDialog("Error", $"Failed to delete message: {error}", "OK");
                    }
                });
            }
        }
        
        #endregion
    }
}