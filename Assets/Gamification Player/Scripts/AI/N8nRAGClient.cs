using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationPlayer.AI
{
    /// <summary>
    /// Client for n8n RAG (Retrieval-Augmented Generation) integration
    /// Handles knowledge retrieval based on conversation context
    /// </summary>
    public class N8nRAGClient
    {
        #region Configuration
        public class Config
        {
            public string endpoint;
            public string apiKey; // Optional, depending on n8n setup
            public int maxResults = 5;
            public float relevanceThreshold = 0.7f;
            
            public bool IsValid()
            {
                return !string.IsNullOrEmpty(endpoint);
            }
        }
        #endregion

        #region Data Structures
        [Serializable]
        public class RAGRequest
        {
            public string query;
            public string[] conversation_history;
            public string user_profile;
            public RAGConfig config;
        }

        [Serializable]
        public class RAGConfig
        {
            public int max_results;
            public float relevance_threshold;
            public string[] categories; // e.g., "legal", "hr", "mindfulness"
        }

        [Serializable]
        public class RAGResponse
        {
            public bool success;
            public RAGResult[] results;
            public string context_summary;
            public string suggested_approach;
            public RAGMetadata metadata;
        }

        [Serializable]
        public class RAGResult
        {
            public string content;
            public float relevance_score;
            public string source;
            public string category;
            public string[] tags;
        }

        [Serializable]
        public class RAGMetadata
        {
            public int total_results;
            public float processing_time;
            public string query_classification;
            public string[] suggested_categories;
        }

        [Serializable]
        public class ErrorResponse
        {
            public string error;
            public string message;
            public int code;
        }
        #endregion

        #region Events
        public static event Action<RAGResponse> OnRAGResponseReceived;
         public static event Action<string> OnRAGErrorOccurred;
        #endregion

        private Config config;
        private MonoBehaviour coroutineRunner;

        public N8nRAGClient(Config config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Query the RAG system for relevant context
        /// </summary>
        /// <param name="query">User's query or conversation context</param>
        /// <param name="conversationHistory">Recent conversation messages</param>
        /// <param name="userProfile">User profile information</param>
        /// <param name="categories">Specific categories to search in</param>
        /// <param name="onComplete">Callback with RAG results or error</param>
        public void QueryRAG(string query, List<string> conversationHistory, string userProfile, 
                            string[] categories, System.Action<RAGResponse, bool> onComplete)
        {
            if (!config.IsValid())
            {
                string error = "n8n RAG endpoint not configured";
                OnRAGErrorOccurred?.Invoke(error);
                onComplete?.Invoke(null, false);
                return;
            }

            coroutineRunner.StartCoroutine(QueryRAGCoroutine(query, conversationHistory, userProfile, categories, onComplete));
        }

        private IEnumerator QueryRAGCoroutine(string query, List<string> conversationHistory, string userProfile,
                                             string[] categories, System.Action<RAGResponse, bool> onComplete)
        {
            var request = new RAGRequest
            {
                query = query,
                conversation_history = conversationHistory?.ToArray() ?? new string[0],
                user_profile = userProfile ?? "",
                config = new RAGConfig
                {
                    max_results = config.maxResults,
                    relevance_threshold = config.relevanceThreshold,
                    categories = categories ?? new string[0]
                }
            };

            string jsonData = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest webRequest = new UnityWebRequest(config.endpoint, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                
                if (!string.IsNullOrEmpty(config.apiKey))
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");
                }

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<RAGResponse>(webRequest.downloadHandler.text);
                        
                        OnRAGResponseReceived?.Invoke(response);
                        onComplete?.Invoke(response, true);
                    }
                    catch (Exception ex)
                    {
                        string error = $"Failed to parse n8n RAG response: {ex.Message}";
                        OnRAGErrorOccurred?.Invoke(error);
                        onComplete?.Invoke(null, false);
                    }
                }
                else
                {
                    string errorMessage = $"n8n RAG API Error: {webRequest.error}";
                    
                    // Try to parse error response
                    if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {
                        try
                        {
                            var errorResponse = JsonUtility.FromJson<ErrorResponse>(webRequest.downloadHandler.text);
                            if (!string.IsNullOrEmpty(errorResponse.message))
                            {
                                errorMessage = $"n8n RAG Error: {errorResponse.message}";
                            }
                        }
                        catch
                        {
                            // Use original error message if parsing fails
                        }
                    }
                    
                    OnRAGErrorOccurred?.Invoke(errorMessage);
                    onComplete?.Invoke(null, false);
                }
            }
        }

        /// <summary>
        /// Classify user query to determine appropriate categories for RAG search
        /// </summary>
        public static string[] ClassifyQuery(string query)
        {
            var categories = new List<string>();
            
            string queryLower = query.ToLower();
            
            // Legal keywords
            if (queryLower.Contains("legal") || queryLower.Contains("law") || queryLower.Contains("rights") ||
                queryLower.Contains("contract") || queryLower.Contains("lawsuit") || queryLower.Contains("attorney"))
            {
                categories.Add("legal");
            }
            
            // HR keywords
            if (queryLower.Contains("hr") || queryLower.Contains("work") || queryLower.Contains("job") ||
                queryLower.Contains("employment") || queryLower.Contains("manager") || queryLower.Contains("colleague"))
            {
                categories.Add("hr");
            }
            
            // Mindfulness/wellness keywords
            if (queryLower.Contains("stress") || queryLower.Contains("anxiety") || queryLower.Contains("mindfulness") ||
                queryLower.Contains("meditation") || queryLower.Contains("wellness") || queryLower.Contains("mental"))
            {
                categories.Add("mindfulness");
            }
            
            // Finance keywords
            if (queryLower.Contains("money") || queryLower.Contains("budget") || queryLower.Contains("financial") ||
                queryLower.Contains("debt") || queryLower.Contains("savings") || queryLower.Contains("investment"))
            {
                categories.Add("finance");
            }
            
            // If no specific category found, use general
            if (categories.Count == 0)
            {
                categories.Add("general");
            }
            
            return categories.ToArray();
        }

        /// <summary>
        /// Extract context summary from RAG response for prompt generation
        /// </summary>
        public static string ExtractContextForPrompt(RAGResponse ragResponse)
        {
            if (ragResponse == null || !ragResponse.success || ragResponse.results == null)
            {
                return "No relevant context available.";
            }

            var contextBuilder = new StringBuilder();
            
            // Add context summary if available
            if (!string.IsNullOrEmpty(ragResponse.context_summary))
            {
                contextBuilder.AppendLine($"Context Summary: {ragResponse.context_summary}");
            }
            
            // Add suggested approach if available
            if (!string.IsNullOrEmpty(ragResponse.suggested_approach))
            {
                contextBuilder.AppendLine($"Suggested Approach: {ragResponse.suggested_approach}");
            }
            
            // Add relevant results
            contextBuilder.AppendLine("Relevant Information:");
            foreach (var result in ragResponse.results)
            {
                if (result.relevance_score >= 0.5f) // Only include highly relevant results
                {
                    contextBuilder.AppendLine($"- {result.content} (Category: {result.category}, Relevance: {result.relevance_score:F2})");
                }
            }
            
            return contextBuilder.ToString();
        }
    }
}