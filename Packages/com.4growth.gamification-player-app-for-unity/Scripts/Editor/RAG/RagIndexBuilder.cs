using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.InferenceEngine;
using GamificationPlayer.Chat.Services;
using System.Linq;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// Editor tool to pre-build RAG indices at design-time for better runtime performance.
    /// Saves indices to StreamingAssets folder for deployment-safe storage.
    /// </summary>
    public class RagIndexBuilder : EditorWindow
    {
        private const string StreamingAssetsRAGPath = "StreamingAssets/RAG/Indices";
        private const string MenuPath = "Tools/Gamification Player/Build RAG Indices";
        
        private Vector2 scrollPosition;
        private bool showValidation = true;
        private bool autoRefreshAssets = true;
        
        [MenuItem(MenuPath)]
        public static void ShowWindow()
        {
            GetWindow<RagIndexBuilder>("RAG Index Builder");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("RAG Index Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox(
                "This tool pre-builds RAG indices at Editor-time to improve runtime performance. " +
                "Indices are saved to StreamingAssets/RAG/Indices for deployment safety.", 
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            // Settings
            showValidation = EditorGUILayout.Toggle("Show Validation Details", showValidation);
            autoRefreshAssets = EditorGUILayout.Toggle("Auto Refresh Assets", autoRefreshAssets);
            
            EditorGUILayout.Space();
            
            // Find all RagSettings
            var ragSettings = FindAllRagSettings();
            
            if (ragSettings.Count == 0)
            {
                EditorGUILayout.HelpBox("No RagSettings found in the project.", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.LabelField($"Found {ragSettings.Count} RagSettings:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var settings in ragSettings)
            {
                DrawRagSettingsEntry(settings);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // Build buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Build All Indices", GUILayout.Height(30)))
            {
                BuildAllIndices(ragSettings);
            }
            
            if (GUILayout.Button("Clean All Indices", GUILayout.Height(30)))
            {
                CleanAllIndices();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Status info
            ShowIndexStatus();
        }

        private void DrawRagSettingsEntry(RagSettings settings)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{settings.agentName} ({settings.rAGType})", EditorStyles.boldLabel);
            
            string indexPath = GetIndexPath(settings);
            bool indexExists = File.Exists(indexPath);
            
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
            statusStyle.normal.textColor = indexExists ? Color.green : Color.red;
            EditorGUILayout.LabelField(indexExists ? "✓ Built" : "✗ Missing", statusStyle, GUILayout.Width(60));
            
            EditorGUILayout.EndHorizontal();
            
            // Validation
            if (showValidation)
            {
                var validation = ValidateRagSettings(settings);
                if (validation.Count > 0)
                {
                    foreach (var issue in validation)
                    {
                        EditorGUILayout.HelpBox(issue, MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ Configuration valid", MessageType.Info);
                }
            }
            
            // Actions
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Build Index", GUILayout.Width(100)))
            {
                BuildSingleIndex(settings);
            }
            
            if (indexExists && GUILayout.Button("Delete Index", GUILayout.Width(100)))
            {
                DeleteSingleIndex(settings);
            }
            
            if (GUILayout.Button("Ping Asset", GUILayout.Width(100)))
            {
                EditorGUIUtility.PingObject(settings);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private List<RagSettings> FindAllRagSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:RagSettings");
            var settings = new List<RagSettings>();
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<RagSettings>(assetPath);
                if (asset != null)
                {
                    settings.Add(asset);
                }
            }
            
            return settings.OrderBy(s => s.agentName).ThenBy(s => s.rAGType.ToString()).ToList();
        }

        private List<string> ValidateRagSettings(RagSettings settings)
        {
            var issues = new List<string>();
            
            if (string.IsNullOrEmpty(settings.agentName))
                issues.Add("Agent Name is required");
                
            if (settings.sourceDocs == null || settings.sourceDocs.Length == 0)
                issues.Add("No source documents assigned");
            else
            {
                foreach (var doc in settings.sourceDocs)
                {
                    if (doc == null)
                        issues.Add("Null document reference found");
                    else if (string.IsNullOrEmpty(doc.text))
                        issues.Add($"Document '{doc.name}' is empty");
                }
            }
            
            if (settings.ChunkMaxTokens <= 0)
                issues.Add("ChunkMaxTokens must be positive");
                
            if (settings.ChunkOverlapTokens < 0)
                issues.Add("ChunkOverlapTokens cannot be negative");
                
            if (settings.ChunkOverlapTokens >= settings.ChunkMaxTokens)
                issues.Add("ChunkOverlapTokens should be less than ChunkMaxTokens");
                
            if (settings.EmbeddingDim <= 0)
                issues.Add("EmbeddingDim must be positive");
                
            return issues;
        }

        private void BuildAllIndices(List<RagSettings> settingsList)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Building RAG Indices", "Preparing...", 0f);
                
                // Ensure StreamingAssets directory exists
                EnsureStreamingAssetsDirectory();
                
                // Find model config
                var modelConfig = FindModelConfig();
                
                if (modelConfig == null)
                {
                    EditorUtility.DisplayDialog("Error", 
                        "Could not find RagModelConfig asset. Please create one using Create > AIChat > RAG Model Config.", 
                        "OK");
                    return;
                }
                
                if (!modelConfig.ValidateFiles(out string configError))
                {
                    EditorUtility.DisplayDialog("Error", 
                        $"Model configuration invalid: {configError}", 
                        "OK");
                    return;
                }
                
                int totalSettings = settingsList.Count;
                int successCount = 0;
                
                for (int i = 0; i < totalSettings; i++)
                {
                    var settings = settingsList[i];
                    float progress = (float)i / totalSettings;
                    
                    EditorUtility.DisplayProgressBar("Building RAG Indices", 
                        $"Building {settings.agentName} ({settings.rAGType})...", progress);
                    
                    if (BuildIndexForSettings(settings, modelConfig))
                    {
                        successCount++;
                    }
                }
                
                EditorUtility.DisplayDialog("RAG Index Builder", 
                    $"Successfully built {successCount}/{totalSettings} indices.", "OK");
                    
                if (autoRefreshAssets)
                {
                    AssetDatabase.Refresh();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void BuildSingleIndex(RagSettings settings)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Building RAG Index", $"Building {settings.agentName}...", 0.5f);
                
                EnsureStreamingAssetsDirectory();
                
                var modelConfig = FindModelConfig();
                
                if (modelConfig == null)
                {
                    EditorUtility.DisplayDialog("Error", 
                        "Could not find RagModelConfig asset.", "OK");
                    return;
                }
                
                if (!modelConfig.ValidateFiles(out string configError))
                {
                    EditorUtility.DisplayDialog("Error", 
                        $"Model configuration invalid: {configError}", "OK");
                    return;
                }
                
                bool success = BuildIndexForSettings(settings, modelConfig);
                
                EditorUtility.DisplayDialog("RAG Index Builder", 
                    success ? "Index built successfully!" : "Failed to build index. Check console for details.", 
                    "OK");
                    
                if (autoRefreshAssets)
                {
                    AssetDatabase.Refresh();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private bool BuildIndexForSettings(RagSettings settings, RagModelConfig modelConfig)
        {
            try
            {
                // Validate settings first
                var validation = ValidateRagSettings(settings);
                if (validation.Count > 0)
                {
                    Debug.LogError($"[RAG Builder] Cannot build index for {settings.agentName}: {string.Join(", ", validation)}");
                    return false;
                }
                
                // Initialize embedder from StreamingAssets
                var embedder = StreamingAssetsMiniLMEmbedder.FromConfig(modelConfig);
                
                // Build chunks
                var chunker = new RagChunker(embedder.TokenizeCount);
                var chunks = new List<RagChunk>();
                
                foreach (var doc in settings.sourceDocs)
                {
                    if (doc == null) continue;
                    
                    string docId = doc.name;
                    string text = doc.text;
                    
                    foreach (var (chunkText, order) in chunker.ChunkByTokens(text, settings.ChunkMaxTokens, settings.ChunkOverlapTokens))
                    {
                        chunks.Add(new RagChunk
                        {
                            DocId = docId,
                            Order = order,
                            Text = chunkText
                        });
                    }
                }
                
                if (chunks.Count == 0)
                {
                    Debug.LogWarning($"[RAG Builder] No chunks generated for {settings.agentName}");
                    return false;
                }
                
                // Embed chunks
                embedder.EmbedInPlace(chunks);
                
                // Create and save index
                var index = new RagIndex(chunks, settings.EmbeddingDim);
                string indexPath = GetIndexPath(settings);
                index.Save(indexPath);
                
                Debug.Log($"[RAG Builder] Built index for {settings.agentName} ({settings.rAGType}): {chunks.Count} chunks -> {indexPath}");
                
                // Cleanup
                embedder?.Dispose();
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RAG Builder] Failed to build index for {settings.agentName}: {ex.Message}");
                return false;
            }
        }

        private void CleanAllIndices()
        {
            if (!EditorUtility.DisplayDialog("Clean All Indices", 
                "Are you sure you want to delete all RAG indices?", "Yes", "Cancel"))
            {
                return;
            }
            
            string ragFolder = Path.Combine(Application.streamingAssetsPath, "RAG", "Indices");
            
            if (Directory.Exists(ragFolder))
            {
                Directory.Delete(ragFolder, true);
                AssetDatabase.Refresh();
                Debug.Log("[RAG Builder] All indices cleaned");
            }
        }

        private void DeleteSingleIndex(RagSettings settings)
        {
            string indexPath = GetIndexPath(settings);
            if (File.Exists(indexPath))
            {
                File.Delete(indexPath);
                AssetDatabase.Refresh();
                Debug.Log($"[RAG Builder] Deleted index: {indexPath}");
            }
        }

        private void ShowIndexStatus()
        {
            string ragFolder = Path.Combine(Application.streamingAssetsPath, "RAG", "Indices");
            
            if (!Directory.Exists(ragFolder))
            {
                EditorGUILayout.HelpBox("No RAG indices directory found in StreamingAssets.", MessageType.Info);
                return;
            }
            
            var indexFiles = Directory.GetFiles(ragFolder, "*.bin");
            
            if (indexFiles.Length == 0)
            {
                EditorGUILayout.HelpBox("No index files found.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"Index files in StreamingAssets: {indexFiles.Length}");
                
                long totalSize = indexFiles.Sum(f => new FileInfo(f).Length);
                EditorGUILayout.LabelField($"Total size: {FormatFileSize(totalSize)}");
            }
            
            // Show model status
            ShowModelStatus();
        }
        
        private void ShowModelStatus()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Model Status", EditorStyles.boldLabel);
            
            var modelConfig = FindModelConfig();
            if (modelConfig == null)
            {
                EditorGUILayout.HelpBox("No RagModelConfig found. Create one with Create > AIChat > RAG Model Config", MessageType.Warning);
                return;
            }
            
            string modelPath = modelConfig.GetModelFullPath();
            string vocabPath = modelConfig.GetVocabFullPath();
            
            // Check both .onnx and .sentis versions
            string onnxPath = Path.ChangeExtension(modelPath, ".onnx");
            string sentisPath = Path.ChangeExtension(modelPath, ".sentis");
            
            bool hasOnnx = File.Exists(onnxPath);
            bool hasSentis = File.Exists(sentisPath);
            bool hasVocab = File.Exists(vocabPath);
            
            if (hasOnnx)
            {
                EditorGUILayout.LabelField($"✓ ONNX Model: {Path.GetFileName(onnxPath)} ({FormatFileSize(new FileInfo(onnxPath).Length)})");
            }
            
            if (hasSentis)
            {
                EditorGUILayout.LabelField($"✓ Sentis Model: {Path.GetFileName(sentisPath)} ({FormatFileSize(new FileInfo(sentisPath).Length)})");
            }
            
            if (hasVocab)
            {
                EditorGUILayout.LabelField($"✓ Vocabulary: {Path.GetFileName(vocabPath)} ({FormatFileSize(new FileInfo(vocabPath).Length)})");
            }
            
            if (!hasOnnx && !hasSentis)
            {
                EditorGUILayout.HelpBox($"❌ No model found at: {Path.GetFileNameWithoutExtension(modelPath)} (.onnx or .sentis)", MessageType.Error);
            }
            
            if (!hasVocab)
            {
                EditorGUILayout.HelpBox($"❌ Vocabulary file missing: {Path.GetFileName(vocabPath)}", MessageType.Error);
            }
        }

        private void EnsureStreamingAssetsDirectory()
        {
            string ragFolder = Path.Combine(Application.streamingAssetsPath, "RAG", "Indices");
            
            if (!Directory.Exists(ragFolder))
            {
                Directory.CreateDirectory(ragFolder);
                AssetDatabase.Refresh();
                Debug.Log($"[RAG Builder] Created directory: {ragFolder}");
            }
        }

        private string GetIndexPath(RagSettings settings)
        {
            return Path.Combine(Application.streamingAssetsPath, "RAG", "Indices", 
                $"rag_index_{settings.agentName}_{settings.rAGType}.bin");
        }

        private RagModelConfig FindModelConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:RagModelConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<RagModelConfig>(path);
            }
            
            return null;
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int suffixIndex = 0;
            double size = bytes;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }
            
            return $"{size:F1} {suffixes[suffixIndex]}";
        }
    }
}