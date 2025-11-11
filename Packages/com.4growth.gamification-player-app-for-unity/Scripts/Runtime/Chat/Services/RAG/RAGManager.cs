using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GamificationPlayer.Chat.Services;
using System.Linq;

namespace GamificationPlayer
{
    /// <summary>
    /// Static RAG manager - no MonoBehaviour dependencies.
    /// Loads indices from StreamingAssets and provides RAG services.
    /// Thread-safe singleton pattern for global access.
    /// </summary>
    public static class RAGManager
    {
        private static readonly object _lock = new object();
        private static Dictionary<string, List<IRAG>> _ragInstances = new Dictionary<string, List<IRAG>>();
        private static RagModelConfig _modelConfig;
        private static IEmbedder _embedder;
        private static bool _isInitialized = false;
        private static string _lastError = null;

        /// <summary>
        /// Initialize the RAG system with model configuration (async for WebGL compatibility).
        /// Must be called before using any RAG functionality.
        /// </summary>
        /// <param name="modelConfig">Configuration for model loading</param>
        /// <param name="ragSettings">Array of RAG settings to load</param>
        /// <returns>True if initialization succeeded</returns>
        public static async System.Threading.Tasks.Task<bool> InitializeAsync(RagModelConfig modelConfig, RagSettings[] ragSettings)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[RAG Manager] Already initialized");
                return true;
            }

            try
            {
                _modelConfig = modelConfig ?? throw new ArgumentNullException(nameof(modelConfig));
                
                if (ragSettings == null || ragSettings.Length == 0)
                {
                    _lastError = "No RAG settings provided";
                    return false;
                }

#if !UNITY_WEBGL || UNITY_EDITOR
                // Desktop: can validate files exist
                if (!_modelConfig.ValidateFiles(out string configError))
                {
                    _lastError = $"Model configuration invalid: {configError}";
                    return false;
                }
#endif
               
                // Initialize embedder asynchronously (WebGL compatible)
                _embedder = await StreamingAssetsMiniLMEmbedder.FromConfigAsync(_modelConfig);

                // Load all RAG instances
                var loadedCount = 0;
                foreach (var settings in ragSettings)
                {
                    try
                    {
                        var rag = LoadRAGFromSettings(settings);
                        if (rag != null)
                        {
                            // Group RAGs by agent name for easy lookup
                            string agentKey = settings.agentName.ToLower();
                            lock (_lock)
                            {
                                if (!_ragInstances.ContainsKey(agentKey))
                                {
                                    _ragInstances[agentKey] = new List<IRAG>();
                                }
                                _ragInstances[agentKey].Add(rag);
                            }
                            loadedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[RAG Manager] Failed to load RAG for {settings.agentName} ({settings.rAGType}): {ex.Message}");
                    }
                }

                _isInitialized = true;
                _lastError = null;

                return loadedCount > 0;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Debug.LogError($"[RAG Manager] Initialization failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Initialize from resources by finding configs automatically (async for WebGL compatibility).
        /// Loads RagModelConfig and all RagSettings from Resources folder.
        /// </summary>
        /// <returns>True if initialization succeeded</returns>
        public static async System.Threading.Tasks.Task<bool> InitializeFromResourcesAsync()
        {
            // Find model config in Resources
            var modelConfigs = Resources.LoadAll<RagModelConfig>("");
            if (modelConfigs.Length == 0)
            {
#if UNITY_EDITOR
                // In editor, try to find in project assets
                var modelConfigGuids = UnityEditor.AssetDatabase.FindAssets("t:RagModelConfig");
                if (modelConfigGuids.Length == 0)
                {
                    _lastError = "No RagModelConfig found in Resources or project";
                    return false;
                }
                
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(modelConfigGuids[0]);
                var modelConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<RagModelConfig>(path);
                
                // Find all RAG settings
                var ragSettingsGuids = UnityEditor.AssetDatabase.FindAssets("t:RagSettings");
                var ragSettings = new List<RagSettings>();
                
                foreach (string guid in ragSettingsGuids)
                {
                    string settingsPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<RagSettings>(settingsPath);
                    if (settings != null)
                        ragSettings.Add(settings);
                }
                
                return await InitializeAsync(modelConfig, ragSettings.ToArray());
#else
                _lastError = "No RagModelConfig found in Resources folder";
                return false;
#endif
            }
            else
            {
                // Use Resources loading
                var modelConfig = modelConfigs[0];
                var ragSettings = Resources.LoadAll<RagSettings>("").ToArray();
                return await InitializeAsync(modelConfig, ragSettings);
            }
        }

        /// <summary>
        /// Synchronous wrapper for editor (deprecated - use InitializeFromResourcesAsync for WebGL compatibility)
        /// </summary>
        [System.Obsolete("Use InitializeFromResourcesAsync for WebGL compatibility")]
        public static bool InitializeFromResources()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            throw new InvalidOperationException("Use InitializeFromResourcesAsync() for WebGL builds");
#else
            return InitializeFromResourcesAsync().GetAwaiter().GetResult();
#endif
        }

        /// <summary>
        /// Create a RAG service with all loaded RAG instances.
        /// </summary>
        /// <returns>RAG service instance, or null if not initialized</returns>
        public static IRAGService CreateRAGService(bool isLoggingEnabled = false)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    Debug.LogWarning("[RAG Manager] Not initialized. Call Initialize() first.");
                    return null;
                }

                var allRags = new List<IRAG>();
                foreach (var ragList in _ragInstances.Values)
                {
                    allRags.AddRange(ragList);
                }

                return new RAGService(allRags, isLoggingEnabled);
            }
        }

        /// <summary>
        /// Get status information about the RAG system.
        /// </summary>
        public static string GetStatusInfo()
        {
            lock (_lock)
            {
                if (!_isInitialized)
                    return $"Not initialized. Error: {_lastError ?? "Unknown"}";

                int totalRags = _ragInstances.Values.Sum(list => list.Count);
                int agentCount = _ragInstances.Count;

                return $"Initialized: {agentCount} agents, {totalRags} RAG instances";
            }
        }

        /// <summary>
        /// Get the last error message if initialization failed.
        /// </summary>
        public static string LastError => _lastError;

        /// <summary>
        /// Shutdown and cleanup all resources.
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                if (!_isInitialized)
                    return;

                // Dispose all RAG instances
                foreach (var ragList in _ragInstances.Values)
                {
                    foreach (var rag in ragList)
                    {
                        rag?.Dispose();
                    }
                }

                _ragInstances.Clear();
                _embedder?.Dispose();
                _embedder = null;
                _modelConfig = null;
                _isInitialized = false;
                _lastError = null;
            }
        }
        /* PRIVATE METHODS */

        private static Rag LoadRAGFromSettings(RagSettings settings)
        {
            // Validate settings
            if (string.IsNullOrEmpty(settings.agentName))
                throw new ArgumentException("Agent name is required");

            try
            {
                // Load pre-built index
                var index = RagIndex.Load(settings.IndexFile);

                // Create RAG runtime (embedder is shared across all RAGs)
                return new Rag(settings.rAGType, settings.agentName, _embedder, index);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load index for {settings.agentName}: {ex.Message}");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only method to initialize from project assets automatically.
        /// </summary>
        [UnityEditor.MenuItem("Tools/Gamification Player/Initialize RAG Manager")]
        private static async void EditorInitialize()
        {
            try
            {
                if (await InitializeFromResourcesAsync())
                {
                    Debug.Log($"[RAG Manager] Editor initialization successful: {GetStatusInfo()}");
                }
                else
                {
                    Debug.LogError($"[RAG Manager] Editor initialization failed: {LastError}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RAG Manager] Editor initialization error: {ex.Message}");
            }
        }

        [UnityEditor.MenuItem("Tools/Gamification Player/Debug RAG Manager Status")]
        private static void EditorDebugStatus()
        {
            Debug.Log($"[RAG Manager] Status: {GetStatusInfo()}");
        }

        [UnityEditor.MenuItem("Tools/Gamification Player/Shutdown RAG Manager")]
        private static void EditorShutdown()
        {
            Shutdown();
        }
#endif
    }
}