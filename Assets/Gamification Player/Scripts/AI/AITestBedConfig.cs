using System;
using UnityEngine;

namespace GamificationPlayer.AI
{
    /// <summary>
    /// Configuration helper for AI TestBed settings
    /// Manages persistent storage of API keys and endpoints
    /// </summary>
    [Serializable]
    public class AITestBedConfig
    {
        [Header("OpenAI Configuration")]
        public string openAIApiKey = "";
        public string openAIModel = "gpt-4o-mini";
        public float temperature = 0.7f;
        public int maxTokens = 1000;
        
        [Header("n8n RAG Configuration")]
        public string n8nRAGEndpoint = "https://wmw.app.n8n.cloud/webhook/router";
        public string n8nApiKey = "";
        public int maxRAGResults = 5;
        public float relevanceThreshold = 0.7f;
        
        [Header("Processing Options")]
        public bool enableAI = true;
        public bool enableProfileGeneration = true;
        public bool enableRAG = true;
        public bool enableParallelProcessing = true;
        
        [Header("UI Settings")]
        public bool showAILogs = true;
        public bool showTokenUsage = true;
        public bool showProcessingTime = true;

        private const string PREFS_KEY = "GamificationPlayer_AITestBed_Config";

        /// <summary>
        /// Check if the configuration is valid for AI operations
        /// </summary>
        public bool IsValidForAI()
        {
            return enableAI && !string.IsNullOrEmpty(openAIApiKey);
        }

        /// <summary>
        /// Check if RAG is properly configured
        /// </summary>
        public bool IsValidForRAG()
        {
            return enableRAG && !string.IsNullOrEmpty(n8nRAGEndpoint);
        }

        /// <summary>
        /// Save configuration to PlayerPrefs
        /// </summary>
        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(this, true);
                PlayerPrefs.SetString(PREFS_KEY, json);
                PlayerPrefs.Save();
                Debug.Log("[AITestBedConfig] Configuration saved successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AITestBedConfig] Failed to save configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Load configuration from PlayerPrefs
        /// </summary>
        public static AITestBedConfig Load()
        {
            try
            {
                if (PlayerPrefs.HasKey(PREFS_KEY))
                {
                    string json = PlayerPrefs.GetString(PREFS_KEY);
                    var config = JsonUtility.FromJson<AITestBedConfig>(json);
                    Debug.Log("[AITestBedConfig] Configuration loaded successfully");
                    return config;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AITestBedConfig] Failed to load configuration: {ex.Message}");
            }
            
            // Return default configuration if loading fails
            Debug.Log("[AITestBedConfig] Using default configuration");
            return new AITestBedConfig();
        }

        /// <summary>
        /// Reset configuration to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            openAIApiKey = "";
            openAIModel = "gpt-4o-mini";
            temperature = 0.7f;
            maxTokens = 1000;
            
            n8nRAGEndpoint = "";
            n8nApiKey = "";
            maxRAGResults = 5;
            relevanceThreshold = 0.7f;
            
            enableAI = true;
            enableProfileGeneration = true;
            enableRAG = true;
            enableParallelProcessing = true;
            
            showAILogs = true;
            showTokenUsage = true;
            showProcessingTime = true;
        }

        /// <summary>
        /// Clear saved configuration
        /// </summary>
        public static void ClearSaved()
        {
            PlayerPrefs.DeleteKey(PREFS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[AITestBedConfig] Saved configuration cleared");
        }

        /// <summary>
        /// Get a summary of the current configuration
        /// </summary>
        public string GetConfigSummary()
        {
            var summary = new System.Text.StringBuilder();
            
            summary.AppendLine("AI TestBed Configuration:");
            summary.AppendLine($"• AI Enabled: {enableAI}");
            summary.AppendLine($"• OpenAI Model: {openAIModel}");
            summary.AppendLine($"• OpenAI Configured: {!string.IsNullOrEmpty(openAIApiKey)}");
            summary.AppendLine($"• RAG Enabled: {enableRAG}");
            summary.AppendLine($"• RAG Configured: {!string.IsNullOrEmpty(n8nRAGEndpoint)}");
            summary.AppendLine($"• Profile Generation: {enableProfileGeneration}");
            summary.AppendLine($"• Parallel Processing: {enableParallelProcessing}");
            
            return summary.ToString();
        }

        /// <summary>
        /// Create OpenAI client configuration from this config
        /// </summary>
        public OpenAIClient.Config CreateOpenAIConfig()
        {
            return new OpenAIClient.Config
            {
                apiKey = openAIApiKey,
                model = openAIModel,
                temperature = temperature,
                maxTokens = maxTokens
            };
        }

        /// <summary>
        /// Create n8n RAG client configuration from this config
        /// </summary>
        public N8nRAGClient.Config CreateRAGConfig()
        {
            return new N8nRAGClient.Config
            {
                endpoint = n8nRAGEndpoint,
                apiKey = n8nApiKey,
                maxResults = maxRAGResults,
                relevanceThreshold = relevanceThreshold
            };
        }

        /// <summary>
        /// Create AI Agent configuration from this config
        /// </summary>
        public AIAgent.Config CreateAIAgentConfig()
        {
            return new AIAgent.Config
            {
                openAIConfig = CreateOpenAIConfig(),
                ragConfig = CreateRAGConfig(),
                enableProfileGeneration = enableProfileGeneration,
                enableRAG = enableRAG && IsValidForRAG(),
                enableParallelProcessing = enableParallelProcessing
            };
        }
    }
}