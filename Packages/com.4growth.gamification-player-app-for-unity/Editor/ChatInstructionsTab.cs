using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// Instructions tab for the Chat Database Manager
    /// Provides UI for managing AI agent instructions with testing and validation capabilities
    /// </summary>
    public class ChatInstructionsTab
    {
        #region Private Fields
        
        private ChatDatabaseAPI api;
        private ChatDataCache dataCache;
        private Vector2 scrollPosition;
        private List<ChatInstructionData> currentInstructions = new List<ChatInstructionData>();
        private bool isLoadingInstructions = false;
        private string searchFilter = "";
        private string agentFilter = "";
        
        // Instruction creation/editing
        private bool showCreateForm = false;
        private bool showEditForm = false;
        private string editingInstructionId = "";
        private string formIdentifier = "";
        private string formContent = "";
        private string formAgent = "";
        
        // Testing functionality
        private bool showTestPanel = false;
        private string testPrompt = "";
        private string testResult = "";
        private bool isTestingInstruction = false;
        
        // Sync functionality
        private bool isSyncingAll = false;
        
        // View options
        private bool groupByAgent = true;
        private bool showContentPreview = true;
        private bool showAdvancedOptions = false;
        
        // Agent types (actual agents in the gamification system)
        private readonly string[] commonAgentTypes = {
            "general",
            "buddy_router",
            "agent_memory",
            "agent_praktisch",
            "agent_mentaal",
            "agent_juridisch",
            "custom"
        };
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the instructions tab
        /// </summary>
        public void Initialize(ChatDatabaseAPI api, ChatDataCache dataCache)
        {
            this.api = api;
            this.dataCache = dataCache;
            RefreshInstructions();
        }
        
        /// <summary>
        /// Draw the instructions tab UI
        /// </summary>
        public void DrawTab()
        {
            EditorGUILayout.BeginVertical();
            
            DrawHeader();
            DrawToolbar();
            
            if (showCreateForm || showEditForm)
            {
                DrawInstructionForm();
                EditorGUILayout.Space();
            }
            
            if (showTestPanel)
            {
                DrawTestingPanel();
                EditorGUILayout.Space();
            }
            
            DrawInstructionsList();
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Set search query for filtering instructions
        /// </summary>
        public void SetSearchQuery(string query)
        {
            if (searchFilter != query)
            {
                searchFilter = query;
                FilterInstructions();
            }
        }
        
        #endregion
        
        #region Private Methods - UI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AI Agent Instructions Management", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // Quick stats
            if (currentInstructions.Count > 0)
            {
                var uniqueAgents = currentInstructions.Where(i => !string.IsNullOrEmpty(i.agent))
                    .Select(i => i.agent).Distinct().Count();
                EditorGUILayout.LabelField($"{currentInstructions.Count} instructions, {uniqueAgents} agents", 
                    EditorStyles.miniLabel, GUILayout.Width(150));
            }
            
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshInstructions();
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
                FilterInstructions();
            }
            
            EditorGUILayout.LabelField("Agent:", GUILayout.Width(45));
            string newAgentFilter = EditorGUILayout.TextField(agentFilter, GUILayout.Width(100));
            if (newAgentFilter != agentFilter)
            {
                agentFilter = newAgentFilter;
                FilterInstructions();
            }
            
            GUILayout.FlexibleSpace();
            
            // View options
            bool newGroupByAgent = EditorGUILayout.Toggle("Group by Agent", groupByAgent, GUILayout.Width(110));
            if (newGroupByAgent != groupByAgent)
            {
                groupByAgent = newGroupByAgent;
                FilterInstructions();
            }
            
            showContentPreview = EditorGUILayout.Toggle("Preview", showContentPreview, GUILayout.Width(70));
            
            // Action buttons
            if (GUILayout.Button("Create", GUILayout.Width(60)))
            {
                StartCreateInstruction();
            }
            
            if (GUILayout.Button("Test", GUILayout.Width(50)))
            {
                showTestPanel = !showTestPanel;
            }
            
            // Sync button - show count of unsynced items
            var unsyncedCount = currentInstructions.Count(i => i.status != "synced");
            if (unsyncedCount > 0)
            {
                GUI.enabled = !isSyncingAll;
                if (GUILayout.Button(isSyncingAll ? "Syncing..." : $"Sync ({unsyncedCount})", GUILayout.Width(80)))
                {
                    SyncAllInstructions();
                }
                GUI.enabled = true;
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("All Synced", GUILayout.Width(80));
                GUI.enabled = true;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        private void DrawInstructionForm()
        {
            bool isEditing = showEditForm;
            string formTitle = isEditing ? "Edit AI Instruction" : "Create New AI Instruction";
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(formTitle, EditorStyles.boldLabel);
            
            // Identifier field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Identifier:", GUILayout.Width(80));
            formIdentifier = EditorGUILayout.TextField(formIdentifier);
            EditorGUILayout.EndHorizontal();
            
            // Agent selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Agent Type:", GUILayout.Width(80));
            
            // Dropdown for common agent types
            int selectedAgentIndex = Array.IndexOf(commonAgentTypes, formAgent);
            if (selectedAgentIndex == -1) selectedAgentIndex = commonAgentTypes.Length - 1; // custom
            
            int newAgentIndex = EditorGUILayout.Popup(selectedAgentIndex, commonAgentTypes, GUILayout.Width(100));
            
            if (newAgentIndex == commonAgentTypes.Length - 1) // custom selected
            {
                formAgent = EditorGUILayout.TextField(formAgent, GUILayout.Width(100));
            }
            else
            {
                formAgent = commonAgentTypes[newAgentIndex];
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Content field
            EditorGUILayout.LabelField("Instruction Content:");
            formContent = DrawConstrainedTextArea(formContent, 100);
            
            // Character count
            EditorGUILayout.LabelField($"Characters: {formContent.Length}", EditorStyles.miniLabel);
            
            // Advanced options
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
            if (showAdvancedOptions)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Instruction Guidelines:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• Be specific and clear about the agent's role", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Define the agent's personality and tone", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Specify any constraints or limitations", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Include examples of desired responses", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
            
            // Form buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button(isEditing ? "Update" : "Create", GUILayout.Width(80)))
            {
                if (isEditing)
                    SaveEditedInstruction();
                else
                    CreateInstruction();
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
        
        private void DrawTestingPanel()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🧪 Instruction Testing", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Test how instructions affect AI behavior with sample prompts:");
            
            EditorGUILayout.LabelField("Test Prompt:");
            testPrompt = DrawConstrainedTextArea(testPrompt, 60);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !string.IsNullOrEmpty(testPrompt.Trim()) && !isTestingInstruction;
            if (GUILayout.Button("Test Instructions", GUILayout.Width(120)))
            {
                TestInstructionsWithPrompt();
            }
            GUI.enabled = true;
            
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                testPrompt = "";
                testResult = "";
            }
            
            GUILayout.FlexibleSpace();
            
            if (isTestingInstruction)
            {
                EditorGUILayout.LabelField("Testing...", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Test result
            if (!string.IsNullOrEmpty(testResult))
            {
                EditorGUILayout.LabelField("Test Result:");
                DrawConstrainedSelectableLabel(testResult, 80);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawInstructionsList()
        {
            if (isLoadingInstructions)
            {
                EditorGUILayout.LabelField("Loading instructions...", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            if (currentInstructions.Count == 0)
            {
                EditorGUILayout.HelpBox("No AI instructions found. Create some instructions to guide AI agent behavior.", MessageType.Info);
                return;
            }
            
            EditorGUILayout.LabelField($"Instructions ({currentInstructions.Count})", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(400));
            
            if (groupByAgent)
            {
                DrawGroupedInstructions();
            }
            else
            {
                DrawFlatInstructions();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawGroupedInstructions()
        {
            var groupedInstructions = currentInstructions
                .GroupBy(i => string.IsNullOrEmpty(i.agent) ? "Unassigned" : i.agent)
                .OrderBy(g => g.Key);
            
            foreach (var group in groupedInstructions)
            {
                EditorGUILayout.BeginVertical("box");
                
                // Group header
                EditorGUILayout.BeginHorizontal();
                string agentIcon = GetAgentIcon(group.Key);
                EditorGUILayout.LabelField($"{agentIcon} {group.Key}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"({group.Count()} instructions)", EditorStyles.miniLabel, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(3);
                
                // Instructions in group
                foreach (var instruction in group.OrderBy(i => i.identifier))
                {
                    DrawInstructionItem(instruction, true);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        
        private void DrawFlatInstructions()
        {
            var sortedInstructions = currentInstructions
                .OrderBy(i => i.agent)
                .ThenBy(i => i.identifier);
            
            foreach (var instruction in sortedInstructions)
            {
                DrawInstructionItem(instruction, false);
            }
        }
        
        private void DrawInstructionItem(ChatInstructionData instruction, bool isGrouped)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();
            
            // Header line
            EditorGUILayout.BeginHorizontal();
            
            // Agent icon (if not grouped)
            if (!isGrouped && !string.IsNullOrEmpty(instruction.agent))
            {
                string agentIcon = GetAgentIcon(instruction.agent);
                EditorGUILayout.LabelField($"{agentIcon} {instruction.agent}", EditorStyles.boldLabel, GUILayout.Width(120));
            }
            
            EditorGUILayout.LabelField($"🏷️ {instruction.identifier}", EditorStyles.boldLabel, GUILayout.Width(isGrouped ? 200 : 150));
            EditorGUILayout.LabelField($"ID: {instruction.id.Substring(0, 8)}...", EditorStyles.miniLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField($"{instruction.createdAt:MM/dd HH:mm}", EditorStyles.miniLabel, GUILayout.Width(80));
            
            // Status indicator
            string statusText = instruction.status == "synced" ? "✅" : 
                               instruction.status == "local" ? "⚠️" : "⭕";
            EditorGUILayout.LabelField(statusText, GUILayout.Width(25));
            
            GUILayout.FlexibleSpace();
            
            // Action buttons
            if (GUILayout.Button("Edit", GUILayout.Width(50)))
            {
                StartEditInstruction(instruction);
            }
            
            if (GUILayout.Button("Test", GUILayout.Width(50)))
            {
                TestSingleInstruction(instruction);
            }
            
            // Show sync button only for unsynced instructions
            if (instruction.status != "synced")
            {
                if (GUILayout.Button("Sync", GUILayout.Width(50)))
                {
                    SyncSingleInstruction(instruction);
                }
            }
            
            if (GUILayout.Button("Duplicate", GUILayout.Width(70)))
            {
                DuplicateInstruction(instruction);
            }
            
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                DeleteInstruction(instruction.id);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Content preview
            if (showContentPreview)
            {
                EditorGUILayout.LabelField("Content:", EditorStyles.miniLabel);
                string contentPreview = instruction.content.Length > 150 ? 
                    instruction.content.Substring(0, 150) + "..." : instruction.content;
                DrawConstrainedSelectableLabel(contentPreview, 50, 100);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            if (!isGrouped)
            {
                EditorGUILayout.Space();
            }
        }
        
        #endregion
        
        #region Private Methods - Helper Functions
        
        /// <summary>
        /// Creates a text area with proper word wrapping, width constraints, and automatic scrolling
        /// </summary>
        // Static field to maintain scroll positions for text areas
        private static Dictionary<string, Vector2> textAreaScrollPositions = new Dictionary<string, Vector2>();
        
        private string DrawConstrainedTextArea(string text, float height, float maxWidthOffset = 50f)
        {
            // Create a unique key for this text area based on calling context
            string scrollKey = $"instruction_textarea_{text?.GetHashCode() ?? 0}";
            
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
        
        private string GetAgentIcon(string agentType)
        {
            if (string.IsNullOrEmpty(agentType)) return "❓";
            
            return agentType.ToLower() switch
            {
                "general" => "🤖",
                "buddy_router" => "🔄",
                "agent_memory" => "🧠",
                "agent_praktisch" => "🔧",
                "agent_mentaal" => "💭",
                "agent_juridisch" => "⚖️",
                "custom" => "⚙️",
                "unassigned" => "❓",
                _ => "⚙️"
            };
        }
        
        #endregion
        
        #region Private Methods - Data Operations
        
        private void RefreshInstructions()
        {
            isLoadingInstructions = true;
            
            api.GetAllInstructions((instructions, error) =>
            {
                isLoadingInstructions = false;
                
                if (error != null)
                {
                    Debug.LogError($"Failed to load instructions: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to load instructions: {error}", "OK");
                    return;
                }
                
                currentInstructions = instructions ?? new List<ChatInstructionData>();
                FilterInstructions();
                
                Debug.Log($"Loaded {currentInstructions.Count} instructions");
            });
        }
        
        private void FilterInstructions()
        {
            var allInstructions = dataCache.GetAllInstructions();
            currentInstructions = allInstructions;
            
            // Apply search filter
            if (!string.IsNullOrEmpty(searchFilter))
            {
                currentInstructions = currentInstructions.Where(i =>
                    i.content.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    i.identifier.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (i.agent != null && i.agent.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }
            
            // Apply agent filter
            if (!string.IsNullOrEmpty(agentFilter))
            {
                currentInstructions = currentInstructions.Where(i =>
                    i.agent != null && i.agent.IndexOf(agentFilter, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }
        }
        
        private void StartCreateInstruction()
        {
            showCreateForm = true;
            showEditForm = false;
            ClearForm();
        }
        
        private void StartEditInstruction(ChatInstructionData instruction)
        {
            showEditForm = true;
            showCreateForm = false;
            editingInstructionId = instruction.id;
            
            formIdentifier = instruction.identifier;
            formContent = instruction.content;
            formAgent = instruction.agent ?? "";
        }
        
        private void ClearForm()
        {
            formIdentifier = "";
            formContent = "";
            formAgent = "";
        }
        
        private void CancelForm()
        {
            showCreateForm = false;
            showEditForm = false;
            editingInstructionId = "";
            ClearForm();
        }
        
        private void CreateInstruction()
        {
            if (string.IsNullOrEmpty(formIdentifier.Trim()) || string.IsNullOrEmpty(formContent.Trim()))
            {
                EditorUtility.DisplayDialog("Invalid Input", "Identifier and Content are required.", "OK");
                return;
            }
            
            var instruction = new ChatInstructionData
            {
                id = Guid.NewGuid().ToString(),
                identifier = formIdentifier.Trim(),
                content = formContent.Trim(),
                agent = string.IsNullOrEmpty(formAgent.Trim()) ? null : formAgent.Trim(),
                createdAt = DateTime.UtcNow,
                lastModified = DateTime.UtcNow,
                status = "new"
            };
            
            api.CreateInstruction(instruction, (success, error) =>
            {
                if (success)
                {
                    Debug.Log($"Instruction '{instruction.identifier}' created successfully");
                    CancelForm();
                    RefreshInstructions();
                }
                else
                {
                    Debug.LogError($"Failed to create instruction: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to create instruction: {error}", "OK");
                }
            });
        }
        
        private void SaveEditedInstruction()
        {
            if (string.IsNullOrEmpty(formIdentifier.Trim()) || string.IsNullOrEmpty(formContent.Trim()))
            {
                EditorUtility.DisplayDialog("Invalid Input", "Identifier and Content are required.", "OK");
                return;
            }
            
            var instruction = currentInstructions.FirstOrDefault(i => i.id == editingInstructionId);
            if (instruction != null)
            {
                instruction.identifier = formIdentifier.Trim();
                instruction.content = formContent.Trim();
                instruction.agent = string.IsNullOrEmpty(formAgent.Trim()) ? null : formAgent.Trim();
                instruction.lastModified = DateTime.UtcNow;
                instruction.status = "modified";
                
                api.UpdateInstruction(instruction, (success, error) =>
                {
                    if (success)
                    {
                        Debug.Log($"Instruction '{instruction.identifier}' updated successfully");
                        CancelForm();
                        RefreshInstructions();
                    }
                    else
                    {
                        Debug.LogError($"Failed to update instruction: {error}");
                        EditorUtility.DisplayDialog("Error", $"Failed to update instruction: {error}", "OK");
                    }
                });
            }
        }
        
        private void DuplicateInstruction(ChatInstructionData original)
        {
            var duplicate = new ChatInstructionData
            {
                id = Guid.NewGuid().ToString(),
                identifier = original.identifier + "_copy",
                content = original.content,
                agent = original.agent,
                createdAt = DateTime.UtcNow,
                lastModified = DateTime.UtcNow,
                status = "new"
            };
            
            api.CreateInstruction(duplicate, (success, error) =>
            {
                if (success)
                {
                    Debug.Log($"Instruction duplicated successfully");
                    RefreshInstructions();
                }
                else
                {
                    Debug.LogError($"Failed to duplicate instruction: {error}");
                    EditorUtility.DisplayDialog("Error", $"Failed to duplicate instruction: {error}", "OK");
                }
            });
        }
        
        private void DeleteInstruction(string instructionId)
        {
            if (EditorUtility.DisplayDialog("Delete Instruction",
                "Are you sure you want to delete this instruction? This action cannot be undone.",
                "Delete", "Cancel"))
            {
                api.DeleteInstruction(instructionId, (success, error) =>
                {
                    if (success)
                    {
                        Debug.Log("Instruction deleted successfully");
                        RefreshInstructions();
                    }
                    else
                    {
                        Debug.LogError($"Failed to delete instruction: {error}");
                        EditorUtility.DisplayDialog("Error", $"Failed to delete instruction: {error}", "OK");
                    }
                });
            }
        }
        
        private void TestSingleInstruction(ChatInstructionData instruction)
        {
            testPrompt = $"Testing instruction: {instruction.identifier}\n\nSample prompt for testing...";
            showTestPanel = true;
        }
        
        private void TestInstructionsWithPrompt()
        {
            isTestingInstruction = true;
            testResult = "";
            
            // Simulate instruction testing (in a real implementation, this would call an AI service)
            EditorApplication.delayCall += () =>
            {
                isTestingInstruction = false;
                testResult = $"Test simulation for prompt: '{testPrompt}'\n\n" +
                           $"Applied instructions: {currentInstructions.Count} active instructions\n" +
                           $"Result: This is a simulated test result. In a real implementation, " +
                           $"this would test how the AI agent responds with the given instructions.";
            };
        }
        
        private void SyncSingleInstruction(ChatInstructionData instruction)
        {
            Debug.Log($"Syncing instruction: {instruction.identifier}");
            
            // Update the instruction to trigger sync
            instruction.lastModified = DateTime.UtcNow;
            instruction.status = "syncing";
            
            api.UpdateInstruction(instruction, (success, error) =>
            {
                if (success)
                {
                    Debug.Log($"Instruction '{instruction.identifier}' synced successfully");
                    RefreshInstructions();
                }
                else
                {
                    Debug.LogError($"Failed to sync instruction: {error}");
                    instruction.status = "local"; // Reset to local if sync failed
                    EditorUtility.DisplayDialog("Sync Error", $"Failed to sync instruction: {error}", "OK");
                }
            });
        }
        
        private void SyncAllInstructions()
        {
            var unsyncedInstructions = currentInstructions.Where(i => i.status != "synced").ToList();
            
            if (unsyncedInstructions.Count == 0)
            {
                Debug.Log("No instructions need syncing");
                return;
            }
            
            isSyncingAll = true;
            Debug.Log($"Syncing {unsyncedInstructions.Count} instructions...");
            
            int syncedCount = 0;
            int totalCount = unsyncedInstructions.Count;
            
            foreach (var instruction in unsyncedInstructions)
            {
                instruction.lastModified = DateTime.UtcNow;
                instruction.status = "syncing";
                
                api.UpdateInstruction(instruction, (success, error) =>
                {
                    if (success)
                    {
                        syncedCount++;
                        Debug.Log($"Synced instruction: {instruction.identifier}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to sync instruction '{instruction.identifier}': {error}");
                        instruction.status = "local"; // Reset to local if sync failed
                    }
                    
                    // Check if this was the last sync operation
                    if (syncedCount + (totalCount - syncedCount) == totalCount)
                    {
                        isSyncingAll = false;
                        Debug.Log($"Bulk sync completed: {syncedCount}/{totalCount} instructions synced");
                        RefreshInstructions();
                        
                        if (syncedCount == totalCount)
                        {
                            EditorUtility.DisplayDialog("Sync Complete", 
                                $"Successfully synced {syncedCount} instructions to the cloud.", "OK");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Sync Partially Complete", 
                                $"Synced {syncedCount} out of {totalCount} instructions. Check console for errors.", "OK");
                        }
                    }
                });
            }
        }
        
        #endregion
    }
}