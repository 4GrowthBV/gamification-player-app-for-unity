using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// Predefined Messages tab for the Chat Database Manager
    /// Provides UI for managing template messages with identifiers and button configurations
    /// </summary>
    public class ChatPredefinedMessagesTab
    {
        #region Private Fields
        
        private ChatDatabaseAPI api;
        private ChatDataCache dataCache;
        private Vector2 scrollPosition;
        private List<ChatPredefinedMessageData> currentMessages = new List<ChatPredefinedMessageData>();
        private bool isLoadingMessages = false;
        private string searchFilter = "";
        private string identifierFilter = "";
        
        // Message creation/editing
        private bool showCreateForm = false;
        private bool showEditForm = false;
        private string editingMessageId = "";
        private string formIdentifier = "";
        private string formContent = "";
        private string formButtonName = "";
        private List<string> formButtons = new List<string>();
        private string newButtonText = "";
        
        // Bulk operations
        private bool showBulkOperations = false;
        private List<string> selectedMessageIds = new List<string>();
        private bool selectAll = false;
        
        // View options
        private bool groupByIdentifier = true;
        private bool showAdvancedSearch = false;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the predefined messages tab
        /// </summary>
        public void Initialize(ChatDatabaseAPI api, ChatDataCache dataCache)
        {
            this.api = api;
            this.dataCache = dataCache;
            RefreshMessages();
        }
        
        /// <summary>
        /// Draw the predefined messages tab UI
        /// </summary>
        public void DrawTab()
        {
            EditorGUILayout.BeginVertical();
            
            DrawHeader();
            DrawToolbar();
            
            if (showCreateForm || showEditForm)
            {
                DrawMessageForm();
                EditorGUILayout.Space();
            }
            
            if (showBulkOperations)
            {
                DrawBulkOperationsPanel();
                EditorGUILayout.Space();
            }
            
            DrawMessagesList();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Set search query for filtering predefined messages
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
            EditorGUILayout.LabelField("Predefined Messages Management", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // Quick stats
            if (currentMessages.Count > 0)
            {
                var uniqueIdentifiers = currentMessages.Select(m => m.identifier).Distinct().Count();
                EditorGUILayout.LabelField($"{currentMessages.Count} messages, {uniqueIdentifiers} identifiers", 
                    EditorStyles.miniLabel, GUILayout.Width(150));
            }
            
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshMessages();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Search controls
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            string newSearchFilter = EditorGUILayout.TextField(searchFilter, GUILayout.Width(150));
            if (newSearchFilter != searchFilter)
            {
                searchFilter = newSearchFilter;
                FilterMessages();
            }
            
            EditorGUILayout.LabelField("Identifier:", GUILayout.Width(60));
            string newIdentifierFilter = EditorGUILayout.TextField(identifierFilter, GUILayout.Width(120));
            if (newIdentifierFilter != identifierFilter)
            {
                identifierFilter = newIdentifierFilter;
                FilterMessages();
            }
            
            GUILayout.FlexibleSpace();
            
            // View options
            bool newGroupByIdentifier = EditorGUILayout.Toggle("Group by ID", groupByIdentifier, GUILayout.Width(80));
            if (newGroupByIdentifier != groupByIdentifier)
            {
                groupByIdentifier = newGroupByIdentifier;
                FilterMessages();
            }
            
            // Action buttons
            if (GUILayout.Button("Create", GUILayout.Width(60)))
            {
                StartCreateMessage();
            }
            
            if (GUILayout.Button("Bulk Ops", GUILayout.Width(70)))
            {
                showBulkOperations = !showBulkOperations;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        private void DrawMessageForm()
        {
            bool isEditing = showEditForm;
            string formTitle = isEditing ? "Edit Predefined Message" : "Create New Predefined Message";
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(formTitle, EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Identifier:", GUILayout.Width(80));
            formIdentifier = EditorGUILayout.TextField(formIdentifier);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Content:");
            formContent = DrawConstrainedTextArea(formContent, 60);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Button Name:", GUILayout.Width(80));
            formButtonName = EditorGUILayout.TextField(formButtonName);
            EditorGUILayout.EndHorizontal();
            
            // Buttons list management
            EditorGUILayout.LabelField("Button List:");
            EditorGUILayout.BeginVertical("box");
            
            // Add new button
            EditorGUILayout.BeginHorizontal();
            newButtonText = EditorGUILayout.TextField(newButtonText, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Add", GUILayout.Width(50)) && !string.IsNullOrEmpty(newButtonText.Trim()))
            {
                formButtons.Add(newButtonText.Trim());
                newButtonText = "";
            }
            EditorGUILayout.EndHorizontal();
            
            // Display existing buttons
            for (int i = formButtons.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {formButtons[i]}", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    formButtons.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (formButtons.Count == 0)
            {
                EditorGUILayout.LabelField("No buttons added", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
            
            // Form buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button(isEditing ? "Update" : "Create", GUILayout.Width(80)))
            {
                if (isEditing)
                    SaveEditedMessage();
                else
                    CreateMessage();
            }
            
            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                CancelForm();
            }
            
            GUILayout.FlexibleSpace();
            
            // Validation info
            if (string.IsNullOrEmpty(formIdentifier.Trim()) || string.IsNullOrEmpty(formContent.Trim()))
            {
                EditorGUILayout.LabelField("Identifier and Content are required", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBulkOperationsPanel()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Bulk Operations", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            bool newSelectAll = EditorGUILayout.Toggle("Select All", selectAll, GUILayout.Width(80));
            if (newSelectAll != selectAll)
            {
                selectAll = newSelectAll;
                if (selectAll)
                {
                    selectedMessageIds.Clear();
                    selectedMessageIds.AddRange(currentMessages.Select(m => m.id));
                }
                else
                {
                    selectedMessageIds.Clear();
                }
            }
            
            EditorGUILayout.LabelField($"{selectedMessageIds.Count} selected", GUILayout.Width(100));
            
            GUILayout.FlexibleSpace();
            
            GUI.enabled = selectedMessageIds.Count > 0;
            
            if (GUILayout.Button("Delete Selected", GUILayout.Width(100)))
            {
                BulkDeleteMessages();
            }
            
            if (GUILayout.Button("Export Selected", GUILayout.Width(100)))
            {
                ExportSelectedMessages();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawMessagesList()
        {
            if (isLoadingMessages)
            {
                EditorGUILayout.LabelField("Loading predefined messages...", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            if (currentMessages.Count == 0)
            {
                EditorGUILayout.HelpBox("No predefined messages found. Create some template messages to get started.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Messages ({currentMessages.Count})", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(400));
            
            if (groupByIdentifier)
            {
                DrawGroupedMessages();
            }
            else
            {
                DrawFlatMessages();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawGroupedMessages()
        {
            var groupedMessages = currentMessages.GroupBy(m => m.identifier).OrderBy(g => g.Key);
            
            foreach (var group in groupedMessages)
            {
                EditorGUILayout.BeginVertical("box");
                
                // Group header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"📁 {group.Key}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"({group.Count()} messages)", EditorStyles.miniLabel, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(3);
                
                // Messages in group
                foreach (var message in group.OrderBy(m => m.createdAt))
                {
                    DrawMessageItem(message, true);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        
        private void DrawFlatMessages()
        {
            var sortedMessages = currentMessages.OrderBy(m => m.identifier).ThenBy(m => m.createdAt);
            
            foreach (var message in sortedMessages)
            {
                DrawMessageItem(message, false);
            }
        }
        
        private void DrawMessageItem(ChatPredefinedMessageData message, bool isGrouped)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            // Selection checkbox for bulk operations
            if (showBulkOperations)
            {
                bool isSelected = selectedMessageIds.Contains(message.id);
                bool wasSelected = isSelected;
                isSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                
                if (isSelected != wasSelected)
                {
                    if (isSelected)
                        selectedMessageIds.Add(message.id);
                    else
                        selectedMessageIds.Remove(message.id);
                }
            }
            
            EditorGUILayout.BeginVertical();
            
            // Header line
            EditorGUILayout.BeginHorizontal();
            
            if (!isGrouped)
            {
                EditorGUILayout.LabelField($"🏷️ {message.identifier}", EditorStyles.boldLabel, GUILayout.Width(150));
            }
            
            EditorGUILayout.LabelField($"ID: {message.id.Substring(0, 8)}...", EditorStyles.miniLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField($"{message.createdAt:MM/dd HH:mm}", EditorStyles.miniLabel, GUILayout.Width(80));
            
            // Status indicator
            string statusText = message.status == "synced" ? "✓" : 
                               message.status == "local" ? "⚠" : "○";
            Color statusColor = message.status == "synced" ? Color.green : 
                               message.status == "local" ? Color.yellow : Color.gray;
            
            Color originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(statusText, GUILayout.Width(20));
            GUI.color = originalColor;
            
            GUILayout.FlexibleSpace();
            
            // Action buttons
            if (GUILayout.Button("Edit", GUILayout.Width(50)))
            {
                StartEditMessage(message);
            }
            
            if (GUILayout.Button("Duplicate", GUILayout.Width(70)))
            {
                DuplicateMessage(message);
            }
            
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                DeleteMessage(message.id);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Content preview
            EditorGUILayout.LabelField("Content:", EditorStyles.miniLabel);
            string contentPreview = message.content.Length > 100 ? 
                message.content.Substring(0, 100) + "..." : message.content;
            DrawConstrainedSelectableLabel(contentPreview, 40, 100);
            
            // Button info
            if (!string.IsNullOrEmpty(message.buttonName) || message.buttons.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (!string.IsNullOrEmpty(message.buttonName))
                {
                    EditorGUILayout.LabelField($"Button: {message.buttonName}", EditorStyles.miniLabel, GUILayout.Width(150));
                }
                
                if (message.buttons.Count > 0)
                {
                    EditorGUILayout.LabelField($"Actions: {string.Join(", ", message.buttons)}", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            if (!isGrouped)
            {
                EditorGUILayout.Space();
            }
        }
        
        // Static field to maintain scroll positions for text areas
        private static Dictionary<string, Vector2> textAreaScrollPositions = new Dictionary<string, Vector2>();
        
        /// <summary>
        /// Creates a text area with proper word wrapping, width constraints, and explicit scrolling
        /// </summary>
        private string DrawConstrainedTextArea(string text, float height, float maxWidthOffset = 50f)
        {
            // Create a unique key for this text area based on calling context
            string scrollKey = $"predefined_msg_textarea_{text?.GetHashCode() ?? 0}";
            
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
        
        private void RefreshMessages()
        {
            isLoadingMessages = true;
            
            api.GetAllPredefinedMessages((messages, error) =>
            {
                isLoadingMessages = false;
                
                if (error != null)
                {
                    Debug.LogError($"Failed to load predefined messages: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to load predefined messages: {error}", "OK");
                    return;
                }
                
                currentMessages = messages ?? new List<ChatPredefinedMessageData>();
                FilterMessages();
                
                Debug.Log($"Loaded {currentMessages.Count} predefined messages");
            });
        }
        
        private void FilterMessages()
        {
            var allMessages = dataCache.GetAllPredefinedMessages();
            currentMessages = allMessages;
            
            // Apply search filter
            if (!string.IsNullOrEmpty(searchFilter))
            {
                currentMessages = currentMessages.Where(m =>
                    m.content.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    m.identifier.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (m.buttonName != null && m.buttonName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }
            
            // Apply identifier filter
            if (!string.IsNullOrEmpty(identifierFilter))
            {
                currentMessages = currentMessages.Where(m =>
                    m.identifier.IndexOf(identifierFilter, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }
        }
        
        private void StartCreateMessage()
        {
            showCreateForm = true;
            showEditForm = false;
            ClearForm();
        }
        
        private void StartEditMessage(ChatPredefinedMessageData message)
        {
            showEditForm = true;
            showCreateForm = false;
            editingMessageId = message.id;
            
            formIdentifier = message.identifier;
            formContent = message.content;
            formButtonName = message.buttonName ?? "";
            formButtons = new List<string>(message.buttons);
        }
        
        private void ClearForm()
        {
            formIdentifier = "";
            formContent = "";
            formButtonName = "";
            formButtons.Clear();
            newButtonText = "";
        }
        
        private void CancelForm()
        {
            showCreateForm = false;
            showEditForm = false;
            editingMessageId = "";
            ClearForm();
        }
        
        private void CreateMessage()
        {
            if (string.IsNullOrEmpty(formIdentifier.Trim()) || string.IsNullOrEmpty(formContent.Trim()))
            {
                EditorUtility.DisplayDialog("Invalid Input", "Identifier and Content are required.", "OK");
                return;
            }
            
            var message = new ChatPredefinedMessageData
            {
                id = Guid.NewGuid().ToString(),
                identifier = formIdentifier.Trim(),
                content = formContent.Trim(),
                buttonName = string.IsNullOrEmpty(formButtonName.Trim()) ? null : formButtonName.Trim(),
                buttons = new List<string>(formButtons),
                createdAt = DateTime.UtcNow,
                lastModified = DateTime.UtcNow,
                status = "new"
            };
            
            api.CreatePredefinedMessage(message, (success, error) =>
            {
                if (success)
                {
                    Debug.Log($"Predefined message '{message.identifier}' created successfully");
                    CancelForm();
                    RefreshMessages();
                }
                else
                {
                    Debug.LogError($"Failed to create predefined message: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to create predefined message: {error}", "OK");
                }
            });
        }
        
        private void SaveEditedMessage()
        {
            if (string.IsNullOrEmpty(formIdentifier.Trim()) || string.IsNullOrEmpty(formContent.Trim()))
            {
                EditorUtility.DisplayDialog("Invalid Input", "Identifier and Content are required.", "OK");
                return;
            }
            
            var message = currentMessages.FirstOrDefault(m => m.id == editingMessageId);
            if (message != null)
            {
                message.identifier = formIdentifier.Trim();
                message.content = formContent.Trim();
                message.buttonName = string.IsNullOrEmpty(formButtonName.Trim()) ? null : formButtonName.Trim();
                message.buttons = new List<string>(formButtons);
                message.lastModified = DateTime.UtcNow;
                message.status = "modified";
                
                api.UpdatePredefinedMessage(message, (success, error) =>
                {
                    if (success)
                    {
                        Debug.Log($"Predefined message '{message.identifier}' updated successfully");
                        CancelForm();
                        RefreshMessages();
                    }
                    else
                    {
                        Debug.LogError($"Failed to update predefined message: {error}");
                        EditorUtility.DisplayDialog("Error", $"Failed to update predefined message: {error}", "OK");
                    }
                });
            }
        }
        
        private void DuplicateMessage(ChatPredefinedMessageData original)
        {
            var duplicate = new ChatPredefinedMessageData
            {
                id = Guid.NewGuid().ToString(),
                identifier = original.identifier + "_copy",
                content = original.content,
                buttonName = original.buttonName,
                buttons = new List<string>(original.buttons),
                createdAt = DateTime.UtcNow,
                lastModified = DateTime.UtcNow,
                status = "new"
            };
            
            api.CreatePredefinedMessage(duplicate, (success, error) =>
            {
                if (success)
                {
                    Debug.Log($"Predefined message duplicated successfully");
                    RefreshMessages();
                }
                else
                {
                    Debug.LogError($"Failed to duplicate predefined message: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to duplicate predefined message: {error}", "OK");
                }
            });
        }
        
        private void DeleteMessage(string messageId)
        {
            if (EditorUtility.DisplayDialog("Delete Predefined Message",
                "Are you sure you want to delete this predefined message? This action cannot be undone.",
                "Delete", "Cancel"))
            {
                api.DeletePredefinedMessage(messageId, (success, error) =>
                {
                    if (success)
                    {
                        Debug.Log("Predefined message deleted successfully");
                        RefreshMessages();
                    }
                    else
                    {
                        Debug.LogError($"Failed to delete predefined message: {error}");
                        EditorUtility.DisplayDialog("Error", $"Failed to delete predefined message: {error}", "OK");
                    }
                });
            }
        }
        
        private void BulkDeleteMessages()
        {
            if (selectedMessageIds.Count == 0) return;
            
            if (EditorUtility.DisplayDialog("Bulk Delete",
                $"Are you sure you want to delete {selectedMessageIds.Count} predefined messages? This action cannot be undone.",
                "Delete All", "Cancel"))
            {
                int successCount = 0;
                int totalCount = selectedMessageIds.Count;
                
                foreach (var messageId in selectedMessageIds.ToList())
                {
                    api.DeletePredefinedMessage(messageId, (success, error) =>
                    {
                        if (success) successCount++;
                        
                        // Check if this was the last operation
                        if (successCount + (totalCount - successCount) == totalCount)
                        {
                            Debug.Log($"Bulk delete completed: {successCount}/{totalCount} messages deleted");
                            selectedMessageIds.Clear();
                            selectAll = false;
                            RefreshMessages();
                        }
                    });
                }
            }
        }
        
        private void ExportSelectedMessages()
        {
            if (selectedMessageIds.Count == 0) return;
            
            var selectedMessages = currentMessages.Where(m => selectedMessageIds.Contains(m.id)).ToList();
            
            var exportData = new
            {
                exportDate = DateTime.UtcNow,
                messageCount = selectedMessages.Count,
                messages = selectedMessages.Select(m => new
                {
                    identifier = m.identifier,
                    content = m.content,
                    buttonName = m.buttonName,
                    buttons = m.buttons,
                    createdAt = m.createdAt
                })
            };
            
            string json = JsonUtility.ToJson(exportData, true);
            string path = EditorUtility.SaveFilePanel("Export Predefined Messages", "", "predefined_messages_export.json", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"Exported {selectedMessages.Count} predefined messages to {path}");
                EditorUtility.DisplayDialog("Export Complete", $"Exported {selectedMessages.Count} messages to {path}", "OK");
            }
        }
        
        #endregion
    }
}