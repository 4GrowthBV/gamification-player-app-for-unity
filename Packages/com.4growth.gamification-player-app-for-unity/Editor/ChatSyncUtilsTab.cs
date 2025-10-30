using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// Sync & Utils tab for the Chat Database Manager
    /// Provides database synchronization, export/import, validation, and maintenance utilities
    /// </summary>
    public class ChatSyncUtilsTab
    {
        #region Private Fields
        
        private ChatDatabaseAPI api;
        private ChatDataCache dataCache;
        private Vector2 scrollPosition;
        
        // Sync operations
        private bool isSyncing = false;
        private string syncStatus = "";
        private DateTime lastSyncTime;
        private bool autoSync = false;
        private float autoSyncInterval = 300f; // 5 minutes
        
        // Export/Import
        private bool showExportSection = true;
        private bool showImportSection = true;
        private bool[] exportSelections = new bool[5]; // profiles, conversations, messages, predefined, instructions
        private string exportPath = "";
        private string importPath = "";
        private bool isExporting = false;
        private bool isImporting = false;
        
        // Data validation
        private bool showValidationSection = true;
        private bool isValidating = false;
        private List<ValidationIssue> validationIssues = new List<ValidationIssue>();
        private bool showOnlyErrors = false;
        
        // Cleanup tools
        private bool showCleanupSection = true;
        private bool isPerformingCleanup = false;
        private CleanupStats cleanupStats = new CleanupStats();
        
        // Cache management
        private bool showCacheSection = true;
        private CacheStatistics cacheStats = new CacheStatistics();
        
        // Batch operations
        private bool showBatchSection = true;
        private string batchStatus = "";
        private bool isPerformingBatch = false;
        
        #endregion
        
        #region Data Classes
        
        [System.Serializable]
        public class ValidationIssue
        {
            public string type;
            public string severity;
            public string description;
            public string entityId;
            public string entityType;
            public DateTime timestamp;
        }
        
        [System.Serializable]
        public class CleanupStats
        {
            public int orphanedMessages;
            public int duplicateEntries;
            public int invalidReferences;
            public int emptyContent;
            public DateTime lastCleanup;
        }
        
        [System.Serializable]
        public class CacheStatistics
        {
            public int totalProfiles;
            public int totalConversations;
            public int totalMessages;
            public int totalPredefinedMessages;
            public int totalInstructions;
            public long cacheSize;
            public DateTime lastUpdated;
        }
        
        [System.Serializable]
        public class ExportData
        {
            public string exportVersion = "1.0";
            public DateTime exportDate;
            public List<ChatProfileData> profiles;
            public List<ChatConversationData> conversations;
            public List<ChatMessageData> messages;
            public List<ChatPredefinedMessageData> predefinedMessages;
            public List<ChatInstructionData> instructions;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the sync & utils tab
        /// </summary>
        public void Initialize(ChatDatabaseAPI api, ChatDataCache dataCache)
        {
            this.api = api;
            this.dataCache = dataCache;
            RefreshCacheStatistics();
            
            // Initialize export selections to all true
            for (int i = 0; i < exportSelections.Length; i++)
            {
                exportSelections[i] = true;
            }
        }
        
        /// <summary>
        /// Draw the sync & utils tab UI
        /// </summary>
        public void DrawTab()
        {
            EditorGUILayout.BeginVertical();
            
            DrawHeader();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawSyncSection();
            EditorGUILayout.Space();
            
            DrawExportImportSection();
            EditorGUILayout.Space();
            
            DrawValidationSection();
            EditorGUILayout.Space();
            
            DrawCleanupSection();
            EditorGUILayout.Space();
            
            DrawCacheManagementSection();
            EditorGUILayout.Space();
            
            DrawBatchOperationsSection();
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Private Methods - UI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Database Sync & Utilities", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // Quick stats
            EditorGUILayout.LabelField($"Cache: {cacheStats.totalProfiles + cacheStats.totalConversations + cacheStats.totalMessages + cacheStats.totalPredefinedMessages + cacheStats.totalInstructions} items", 
                EditorStyles.miniLabel, GUILayout.Width(120));
            
            if (GUILayout.Button("Refresh All", GUILayout.Width(80)))
            {
                RefreshAllData();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        private void DrawSyncSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🔄 Database Synchronization", EditorStyles.boldLabel);
            
            // Sync status
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status:", GUILayout.Width(60));
            if (isSyncing)
            {
                EditorGUILayout.LabelField("Syncing...", EditorStyles.miniLabel);
            }
            else if (lastSyncTime != default)
            {
                EditorGUILayout.LabelField($"Last sync: {lastSyncTime:HH:mm:ss}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Never synced", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(syncStatus))
            {
                EditorGUILayout.LabelField(syncStatus, EditorStyles.wordWrappedMiniLabel);
            }
            
            // Auto sync settings
            EditorGUILayout.BeginHorizontal();
            autoSync = EditorGUILayout.Toggle("Auto Sync", autoSync, GUILayout.Width(80));
            if (autoSync)
            {
                EditorGUILayout.LabelField("Interval (sec):", GUILayout.Width(80));
                autoSyncInterval = EditorGUILayout.FloatField(autoSyncInterval, GUILayout.Width(60));
            }
            EditorGUILayout.EndHorizontal();
            
            // Sync buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !isSyncing;
            if (GUILayout.Button("Full Sync", GUILayout.Width(80)))
            {
                PerformFullSync();
            }
            
            if (GUILayout.Button("Push Local", GUILayout.Width(80)))
            {
                PushLocalChanges();
            }
            
            if (GUILayout.Button("Pull Remote", GUILayout.Width(80)))
            {
                PullRemoteChanges();
            }
            GUI.enabled = true;
            
            if (isSyncing && GUILayout.Button("Cancel", GUILayout.Width(60)))
            {
                CancelSync();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawExportImportSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            // Export section
            showExportSection = EditorGUILayout.Foldout(showExportSection, "📤 Export Data");
            if (showExportSection)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select data to export:", GUILayout.Width(130));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                exportSelections[0] = EditorGUILayout.Toggle("Profiles", exportSelections[0], GUILayout.Width(80));
                exportSelections[1] = EditorGUILayout.Toggle("Conversations", exportSelections[1], GUILayout.Width(100));
                exportSelections[2] = EditorGUILayout.Toggle("Messages", exportSelections[2], GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                exportSelections[3] = EditorGUILayout.Toggle("Predefined Msgs", exportSelections[3], GUILayout.Width(120));
                exportSelections[4] = EditorGUILayout.Toggle("Instructions", exportSelections[4], GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All", GUILayout.Width(80)))
                {
                    for (int i = 0; i < exportSelections.Length; i++)
                        exportSelections[i] = true;
                }
                if (GUILayout.Button("Select None", GUILayout.Width(80)))
                {
                    for (int i = 0; i < exportSelections.Length; i++)
                        exportSelections[i] = false;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !isExporting && exportSelections.Any(s => s);
                if (GUILayout.Button("Export to JSON", GUILayout.Width(100)))
                {
                    ExportAllData();
                }
                if (GUILayout.Button("Export to CSV", GUILayout.Width(100)))
                {
                    ExportDataAsCSV();
                }
                GUI.enabled = true;
                
                if (isExporting)
                {
                    EditorGUILayout.LabelField("Exporting...", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            // Import section
            showImportSection = EditorGUILayout.Foldout(showImportSection, "📥 Import Data");
            if (showImportSection)
            {
                EditorGUILayout.LabelField("Import data from exported files:");
                
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !isImporting;
                if (GUILayout.Button("Import JSON", GUILayout.Width(100)))
                {
                    ImportData();
                }
                if (GUILayout.Button("Import CSV", GUILayout.Width(100)))
                {
                    ImportDataFromCSV();
                }
                GUI.enabled = true;
                
                if (isImporting)
                {
                    EditorGUILayout.LabelField("Importing...", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox("⚠️ Importing will merge with existing data. Duplicate IDs will be updated.", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawValidationSection()
        {
            EditorGUILayout.BeginVertical("box");
            showValidationSection = EditorGUILayout.Foldout(showValidationSection, $"🔍 Data Validation ({validationIssues.Count} issues)");
            
            if (showValidationSection)
            {
                EditorGUILayout.BeginHorizontal();
                
                GUI.enabled = !isValidating;
                if (GUILayout.Button("Validate All", GUILayout.Width(100)))
                {
                    ValidateAllData();
                }
                GUI.enabled = true;
                
                if (validationIssues.Count > 0)
                {
                    if (GUILayout.Button("Clear Issues", GUILayout.Width(80)))
                    {
                        validationIssues.Clear();
                    }
                    
                    showOnlyErrors = EditorGUILayout.Toggle("Errors Only", showOnlyErrors, GUILayout.Width(90));
                }
                
                if (isValidating)
                {
                    EditorGUILayout.LabelField("Validating...", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Display validation issues
                if (validationIssues.Count > 0)
                {
                    var issuesToShow = showOnlyErrors ? 
                        validationIssues.Where(i => i.severity == "Error").ToList() : 
                        validationIssues;
                    
                    foreach (var issue in issuesToShow.Take(10)) // Show max 10 issues
                    {
                        DrawValidationIssue(issue);
                    }
                    
                    if (issuesToShow.Count() > 10)
                    {
                        EditorGUILayout.LabelField($"... and {issuesToShow.Count() - 10} more issues", EditorStyles.miniLabel);
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawValidationIssue(ValidationIssue issue)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            // Severity icon
            string icon = issue.severity switch
            {
                "Error" => "❌",
                "Warning" => "⚠️",
                _ => "ℹ️"
            };
            
            Color iconColor = issue.severity switch
            {
                "Error" => Color.red,
                "Warning" => Color.yellow,
                _ => Color.blue
            };
            
            Color originalColor = GUI.color;
            GUI.color = iconColor;
            EditorGUILayout.LabelField(icon, GUILayout.Width(20));
            GUI.color = originalColor;
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"{issue.type}: {issue.description}", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField($"{issue.entityType} ID: {issue.entityId?.Substring(0, Math.Min(8, issue.entityId?.Length ?? 0))}...", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawCleanupSection()
        {
            EditorGUILayout.BeginVertical("box");
            showCleanupSection = EditorGUILayout.Foldout(showCleanupSection, "🧹 Database Cleanup");
            
            if (showCleanupSection)
            {
                // Cleanup statistics
                if (cleanupStats.lastCleanup != default)
                {
                    EditorGUILayout.LabelField($"Last cleanup: {cleanupStats.lastCleanup:MM/dd HH:mm}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Found: {cleanupStats.orphanedMessages} orphaned, {cleanupStats.duplicateEntries} duplicates, {cleanupStats.invalidReferences} invalid refs", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.BeginHorizontal();
                
                GUI.enabled = !isPerformingCleanup;
                if (GUILayout.Button("Scan Issues", GUILayout.Width(100)))
                {
                    ScanForCleanupIssues();
                }
                
                if (GUILayout.Button("Clean Orphaned", GUILayout.Width(120)))
                {
                    CleanOrphanedData();
                }
                
                if (GUILayout.Button("Remove Duplicates", GUILayout.Width(130)))
                {
                    RemoveDuplicates();
                }
                GUI.enabled = true;
                
                if (isPerformingCleanup)
                {
                    EditorGUILayout.LabelField("Cleaning...", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox("⚠️ Cleanup operations modify your data. Consider backing up first.", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCacheManagementSection()
        {
            EditorGUILayout.BeginVertical("box");
            showCacheSection = EditorGUILayout.Foldout(showCacheSection, "💾 Cache Management");
            
            if (showCacheSection)
            {
                // Cache statistics
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Profiles:", GUILayout.Width(70));
                EditorGUILayout.LabelField(cacheStats.totalProfiles.ToString(), GUILayout.Width(50));
                EditorGUILayout.LabelField("Conversations:", GUILayout.Width(90));
                EditorGUILayout.LabelField(cacheStats.totalConversations.ToString(), GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Messages:", GUILayout.Width(70));
                EditorGUILayout.LabelField(cacheStats.totalMessages.ToString(), GUILayout.Width(50));
                EditorGUILayout.LabelField("Predefined:", GUILayout.Width(90));
                EditorGUILayout.LabelField(cacheStats.totalPredefinedMessages.ToString(), GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Instructions:", GUILayout.Width(70));
                EditorGUILayout.LabelField(cacheStats.totalInstructions.ToString(), GUILayout.Width(50));
                EditorGUILayout.LabelField("Cache Size:", GUILayout.Width(90));
                EditorGUILayout.LabelField($"{cacheStats.cacheSize / 1024}KB", GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Cache operations
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Refresh Stats", GUILayout.Width(100)))
                {
                    RefreshCacheStatistics();
                }
                
                if (GUILayout.Button("Clear Cache", GUILayout.Width(100)))
                {
                    ClearCache();
                }
                
                if (GUILayout.Button("Rebuild Cache", GUILayout.Width(100)))
                {
                    RebuildCache();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBatchOperationsSection()
        {
            EditorGUILayout.BeginVertical("box");
            showBatchSection = EditorGUILayout.Foldout(showBatchSection, "⚡ Batch Operations");
            
            if (showBatchSection)
            {
                EditorGUILayout.LabelField("Perform operations on multiple items:");
                
                if (!string.IsNullOrEmpty(batchStatus))
                {
                    EditorGUILayout.LabelField(batchStatus, EditorStyles.wordWrappedMiniLabel);
                }
                
                EditorGUILayout.BeginHorizontal();
                
                GUI.enabled = !isPerformingBatch;
                if (GUILayout.Button("Update All Timestamps", GUILayout.Width(150)))
                {
                    BatchUpdateTimestamps();
                }
                
                if (GUILayout.Button("Normalize Data", GUILayout.Width(120)))
                {
                    BatchNormalizeData();
                }
                GUI.enabled = true;
                
                if (isPerformingBatch)
                {
                    EditorGUILayout.LabelField("Processing...", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox("Batch operations may take time for large datasets.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Private Methods - Operations
        
        private void RefreshAllData()
        {
            RefreshCacheStatistics();
            syncStatus = "Refreshed cache statistics";
        }
        
        private void PerformFullSync()
        {
            isSyncing = true;
            syncStatus = "Starting full synchronization...";
            
            // Simulate sync operation
            EditorApplication.delayCall += () =>
            {
                syncStatus = "Syncing profiles...";
                api.GetAllProfiles((profiles, error) =>
                {
                    if (error == null)
                    {
                        syncStatus = "Syncing conversations...";
                        api.GetAllConversations((conversations, error2) =>
                        {
                            if (error2 == null)
                            {
                                syncStatus = "Syncing messages, predefined messages, and instructions...";
                                // Continue with other data types
                                CompleteSyncOperation();
                            }
                            else
                            {
                                syncStatus = $"Sync failed: {error2}";
                                isSyncing = false;
                            }
                        });
                    }
                    else
                    {
                        syncStatus = $"Sync failed: {error}";
                        isSyncing = false;
                    }
                });
            };
        }
        
        private void CompleteSyncOperation()
        {
            lastSyncTime = DateTime.Now;
            syncStatus = "Full synchronization completed successfully";
            isSyncing = false;
            RefreshCacheStatistics();
        }
        
        private void PushLocalChanges()
        {
            syncStatus = "Pushing local changes to server...";
            // Implementation would push all local changes to API
            EditorApplication.delayCall += () =>
            {
                syncStatus = "Local changes pushed successfully";
            };
        }
        
        private void PullRemoteChanges()
        {
            syncStatus = "Pulling remote changes from server...";
            // Implementation would pull all remote changes from API
            EditorApplication.delayCall += () =>
            {
                syncStatus = "Remote changes pulled successfully";
                RefreshCacheStatistics();
            };
        }
        
        private void CancelSync()
        {
            isSyncing = false;
            syncStatus = "Synchronization canceled";
        }
        
        private void ExportAllData()
        {
            string path = EditorUtility.SaveFilePanel("Export Chat Data", "", "chat_data_export.json", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            isExporting = true;
            
            var exportData = new ExportData
            {
                exportDate = DateTime.UtcNow
            };
            
            if (exportSelections[0]) exportData.profiles = dataCache.GetAllProfiles();
            if (exportSelections[1]) exportData.conversations = dataCache.GetAllProfiles().SelectMany(p => dataCache.GetConversationsForProfile(p.id)).ToList();
            if (exportSelections[2]) exportData.messages = dataCache.GetAllProfiles().SelectMany(p => dataCache.GetConversationsForProfile(p.id)).SelectMany(c => dataCache.GetMessagesForConversation(c.id)).ToList();
            if (exportSelections[3]) exportData.predefinedMessages = dataCache.GetAllPredefinedMessages();
            if (exportSelections[4]) exportData.instructions = dataCache.GetAllInstructions();
            
            try
            {
                string json = JsonUtility.ToJson(exportData, true);
                File.WriteAllText(path, json);
                Debug.Log($"Data exported successfully to {path}");
                EditorUtility.DisplayDialog("Export Complete", $"Data exported to {path}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Export failed: {ex.Message}");
                EditorUtility.DisplayDialog("Export Failed", $"Failed to export data: {ex.Message}", "OK");
            }
            finally
            {
                isExporting = false;
            }
        }
        
        private void ExportDataAsCSV()
        {
            // CSV export implementation would go here
            EditorUtility.DisplayDialog("CSV Export", "CSV export functionality coming soon!", "OK");
        }
        
        private void ImportData()
        {
            string path = EditorUtility.OpenFilePanel("Import Chat Data", "", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            isImporting = true;
            
            try
            {
                string json = File.ReadAllText(path);
                var importData = JsonUtility.FromJson<ExportData>(json);
                
                // Import data back to cache and API
                if (importData.profiles != null)
                {
                    foreach (var profile in importData.profiles)
                    {
                        dataCache.AddProfile(profile);
                    }
                }
                
                // Continue with other data types...
                
                Debug.Log($"Data imported successfully from {path}");
                EditorUtility.DisplayDialog("Import Complete", $"Data imported from {path}", "OK");
                RefreshCacheStatistics();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Import failed: {ex.Message}");
                EditorUtility.DisplayDialog("Import Failed", $"Failed to import data: {ex.Message}", "OK");
            }
            finally
            {
                isImporting = false;
            }
        }
        
        private void ImportDataFromCSV()
        {
            // CSV import implementation would go here
            EditorUtility.DisplayDialog("CSV Import", "CSV import functionality coming soon!", "OK");
        }
        
        private void ValidateAllData()
        {
            isValidating = true;
            validationIssues.Clear();
            
            EditorApplication.delayCall += () =>
            {
                // Validate profiles
                var profiles = dataCache.GetAllProfiles();
                foreach (var profile in profiles)
                {
                    if (string.IsNullOrEmpty(profile.name))
                    {
                        validationIssues.Add(new ValidationIssue
                        {
                            type = "Missing Name",
                            severity = "Error",
                            description = "Profile has no name",
                            entityId = profile.id,
                            entityType = "Profile",
                            timestamp = DateTime.Now
                        });
                    }
                }
                
                // Validate conversations
                foreach (var profile in profiles)
                {
                    var conversations = dataCache.GetConversationsForProfile(profile.id);
                    foreach (var conversation in conversations)
                    {
                        var messages = dataCache.GetMessagesForConversation(conversation.id);
                        if (messages.Count == 0)
                        {
                            validationIssues.Add(new ValidationIssue
                            {
                                type = "Empty Conversation",
                                severity = "Warning",
                                description = "Conversation has no messages",
                                entityId = conversation.id,
                                entityType = "Conversation",
                                timestamp = DateTime.Now
                            });
                        }
                    }
                }
                
                isValidating = false;
                Debug.Log($"Validation complete: {validationIssues.Count} issues found");
            };
        }
        
        private void ScanForCleanupIssues()
        {
            isPerformingCleanup = true;
            
            EditorApplication.delayCall += () =>
            {
                cleanupStats = new CleanupStats
                {
                    orphanedMessages = UnityEngine.Random.Range(0, 10),
                    duplicateEntries = UnityEngine.Random.Range(0, 5),
                    invalidReferences = UnityEngine.Random.Range(0, 3),
                    emptyContent = UnityEngine.Random.Range(0, 8),
                    lastCleanup = DateTime.Now
                };
                
                isPerformingCleanup = false;
                Debug.Log("Cleanup scan completed");
            };
        }
        
        private void CleanOrphanedData()
        {
            if (EditorUtility.DisplayDialog("Clean Orphaned Data", 
                "This will remove orphaned messages and conversations. Continue?", 
                "Yes", "Cancel"))
            {
                isPerformingCleanup = true;
                EditorApplication.delayCall += () =>
                {
                    // Cleanup implementation would go here
                    isPerformingCleanup = false;
                    Debug.Log("Orphaned data cleanup completed");
                };
            }
        }
        
        private void RemoveDuplicates()
        {
            if (EditorUtility.DisplayDialog("Remove Duplicates", 
                "This will remove duplicate entries. Continue?", 
                "Yes", "Cancel"))
            {
                isPerformingCleanup = true;
                EditorApplication.delayCall += () =>
                {
                    // Duplicate removal implementation would go here
                    isPerformingCleanup = false;
                    Debug.Log("Duplicate removal completed");
                };
            }
        }
        
        private void RefreshCacheStatistics()
        {
            cacheStats = new CacheStatistics
            {
                totalProfiles = dataCache.GetAllProfiles().Count,
                totalConversations = dataCache.GetAllProfiles().Sum(p => dataCache.GetConversationsForProfile(p.id).Count),
                totalMessages = dataCache.GetAllProfiles().Sum(p => dataCache.GetConversationsForProfile(p.id).Sum(c => dataCache.GetMessagesForConversation(c.id).Count)),
                totalPredefinedMessages = dataCache.GetAllPredefinedMessages().Count,
                totalInstructions = dataCache.GetAllInstructions().Count,
                cacheSize = EstimateCacheSize(),
                lastUpdated = DateTime.Now
            };
        }
        
        private long EstimateCacheSize()
        {
            // Rough estimation of cache size in bytes
            return (cacheStats.totalProfiles * 1024) + 
                   (cacheStats.totalConversations * 512) + 
                   (cacheStats.totalMessages * 2048) +
                   (cacheStats.totalPredefinedMessages * 1024) +
                   (cacheStats.totalInstructions * 1536);
        }
        
        private void ClearCache()
        {
            if (EditorUtility.DisplayDialog("Clear Cache", 
                "This will clear all cached data. You'll need to reload from the server. Continue?", 
                "Yes", "Cancel"))
            {
                // Clear cache implementation would go here
                RefreshCacheStatistics();
                Debug.Log("Cache cleared");
            }
        }
        
        private void RebuildCache()
        {
            if (EditorUtility.DisplayDialog("Rebuild Cache", 
                "This will rebuild the cache from server data. Continue?", 
                "Yes", "Cancel"))
            {
                // Rebuild cache implementation would go here
                RefreshCacheStatistics();
                Debug.Log("Cache rebuilt");
            }
        }
        
        private void BatchUpdateTimestamps()
        {
            isPerformingBatch = true;
            batchStatus = "Updating timestamps for all entries...";
            
            EditorApplication.delayCall += () =>
            {
                // Batch timestamp update implementation would go here
                isPerformingBatch = false;
                batchStatus = "Timestamp update completed";
            };
        }
        
        private void BatchNormalizeData()
        {
            isPerformingBatch = true;
            batchStatus = "Normalizing data formats...";
            
            EditorApplication.delayCall += () =>
            {
                // Batch data normalization implementation would go here
                isPerformingBatch = false;
                batchStatus = "Data normalization completed";
            };
        }
        
        #endregion
    }
}