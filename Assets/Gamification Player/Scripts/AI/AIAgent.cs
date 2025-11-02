using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GamificationPlayer.AI
{
    /// <summary>
    /// AI Agent that coordinates profile generation, RAG retrieval, and OpenAI response generation
    /// Handles the complete AI pipeline for generating contextual responses
    /// </summary>
    public class AIAgent
    {
        #region Configuration
        public class Config
        {
            public OpenAIClient.Config openAIConfig;
            public N8nRAGClient.Config ragConfig;
            public bool enableProfileGeneration = true;
            public bool enableRAG = true;
            public bool enableParallelProcessing = true;
            
            public bool IsValid()
            {
                return openAIConfig != null && openAIConfig.IsValid();
            }
        }
        #endregion

        #region Data Structures
        [Serializable]
        public class AIRequest
        {
            public string userMessage;
            public List<string> conversationHistory;
            public string existingProfile;
            public string conversationContext;
            public string[] categories;
        }

        [Serializable]
        public class AIResponse
        {
            public string message;
            public string updatedProfile;
            public string[] ragCategories;
            public int tokensUsed;
            public float processingTime;
            public bool success;
            public string error;
        }

        [Serializable]
        public class ProfileData
        {
            public string personalityType;
            public string[] interests;
            public string[] concerns;
            public string communicationStyle;
            public string currentEmotionalState;
            public string[] supportNeeds;
            public DateTime lastUpdated;
        }
        #endregion

        #region Events
        public static event Action<AIResponse> OnAIResponseGenerated;
        public static event Action<string> OnProfileUpdated;
        public static event Action<string> OnProcessingStatusChanged;
        public static event Action<string> OnAIErrorOccurred;
        #endregion

        private Config config;
        private OpenAIClient openAIClient;
        private N8nRAGClient ragClient;
        private MonoBehaviour coroutineRunner;

        public AIAgent(Config config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
            
            // Initialize clients
            if (config.openAIConfig != null)
            {
                openAIClient = new OpenAIClient(config.openAIConfig, coroutineRunner);
            }
            
            if (config.ragConfig != null && config.ragConfig.IsValid())
            {
                ragClient = new N8nRAGClient(config.ragConfig, coroutineRunner);
            }
        }

        /// <summary>
        /// Process a user request through the complete AI pipeline
        /// </summary>
        public void ProcessRequest(AIRequest request, System.Action<AIResponse> onComplete)
        {
            if (!config.IsValid())
            {
                var errorResponse = new AIResponse
                {
                    success = false,
                    error = "AI Agent not properly configured"
                };
                OnAIErrorOccurred?.Invoke(errorResponse.error);
                onComplete?.Invoke(errorResponse);
                return;
            }

            // Wrap in try-catch at this level to handle any exceptions
            try
            {
                coroutineRunner.StartCoroutine(ProcessRequestCoroutine(request, onComplete));
            }
            catch (Exception ex)
            {
                var errorResponse = new AIResponse
                {
                    success = false,
                    error = $"Failed to start AI processing: {ex.Message}"
                };
                OnAIErrorOccurred?.Invoke(errorResponse.error);
                onComplete?.Invoke(errorResponse);
            }
        }

        private IEnumerator ProcessRequestCoroutine(AIRequest request, System.Action<AIResponse> onComplete)
        {
            var response = new AIResponse { success = false };
            var startTime = Time.realtimeSinceStartup;
            
            OnProcessingStatusChanged?.Invoke("Starting AI processing...");

            // Determine categories for RAG search
            string[] categories = request.categories ?? N8nRAGClient.ClassifyQuery(request.userMessage);
            response.ragCategories = categories;

            string generatedProfile = null;
            N8nRAGClient.RAGResponse ragResponse = null;
            string errorMessage = null;

            // Use nested coroutines without try-catch to avoid yield issues
            yield return coroutineRunner.StartCoroutine(ProcessAIRequestSafely(request, categories, 
                (profile, rag, error) => {
                    generatedProfile = profile;
                    ragResponse = rag;
                    errorMessage = error;
                }));

            if (!string.IsNullOrEmpty(errorMessage))
            {
                response.error = errorMessage;
                OnAIErrorOccurred?.Invoke(response.error);
                OnProcessingStatusChanged?.Invoke("AI processing failed");
                onComplete?.Invoke(response);
                yield break;
            }

            // Generate final response with OpenAI
            OnProcessingStatusChanged?.Invoke("Generating AI response...");
            
            string finalPrompt = BuildFinalPrompt(request, generatedProfile, ragResponse);
            bool responseSuccess = false;
            string aiResponse = null;
            int tokensUsed = 0;

            yield return coroutineRunner.StartCoroutine(GenerateOpenAIResponseCoroutine(finalPrompt, request.conversationHistory, 
                (responseText, tokens, success) => {
                    aiResponse = responseText;
                    tokensUsed = tokens;
                    responseSuccess = success;
                }));

            // Build final response
            response.success = responseSuccess;
            response.message = aiResponse;
            response.updatedProfile = generatedProfile ?? request.existingProfile;
            response.tokensUsed = tokensUsed;
            response.processingTime = Time.realtimeSinceStartup - startTime;

            if (responseSuccess)
            {
                OnProcessingStatusChanged?.Invoke("AI processing completed successfully");
                OnAIResponseGenerated?.Invoke(response);
                
                if (!string.IsNullOrEmpty(generatedProfile))
                {
                    OnProfileUpdated?.Invoke(generatedProfile);
                }
            }
            else
            {
                response.error = aiResponse; // Error message is in the response text
                OnAIErrorOccurred?.Invoke(response.error);
            }

            onComplete?.Invoke(response);
        }

        private IEnumerator ProcessAIRequestSafely(AIRequest request, string[] categories, 
                                                  System.Action<string, N8nRAGClient.RAGResponse, string> onComplete)
        {
            string generatedProfile = null;
            N8nRAGClient.RAGResponse ragResponse = null;
            
            if (config.enableParallelProcessing)
            {
                // Run profile generation and RAG in parallel
                OnProcessingStatusChanged?.Invoke("Running profile generation and RAG in parallel...");
                
                var profileCoroutine = config.enableProfileGeneration ? 
                    coroutineRunner.StartCoroutine(GenerateProfileCoroutine(request, (profile) => generatedProfile = profile)) : null;
                
                var ragCoroutine = config.enableRAG && ragClient != null ? 
                    coroutineRunner.StartCoroutine(GetRAGContextCoroutine(request, categories, (rag) => ragResponse = rag)) : null;

                // Wait for both to complete
                if (profileCoroutine != null)
                    yield return profileCoroutine;
                if (ragCoroutine != null)
                    yield return ragCoroutine;
            }
            else
            {
                // Run sequentially
                if (config.enableProfileGeneration)
                {
                    OnProcessingStatusChanged?.Invoke("Generating user profile...");
                    yield return coroutineRunner.StartCoroutine(GenerateProfileCoroutine(request, (profile) => generatedProfile = profile));
                }

                if (config.enableRAG && ragClient != null)
                {
                    OnProcessingStatusChanged?.Invoke("Retrieving relevant context...");
                    yield return coroutineRunner.StartCoroutine(GetRAGContextCoroutine(request, categories, (rag) => ragResponse = rag));
                }
            }
            
            onComplete?.Invoke(generatedProfile, ragResponse, null);
        }

        private IEnumerator GenerateProfileCoroutine(AIRequest request, System.Action<string> onComplete)
        {
            var profileMessages = new List<OpenAIClient.Message>
            {
                OpenAIClient.CreateSystemMessage(GetProfileGenerationPrompt()),
                OpenAIClient.CreateUserMessage($"User message: {request.userMessage}\n\nConversation history: {string.Join("\n", request.conversationHistory ?? new List<string>())}\n\nExisting profile: {request.existingProfile ?? "None"}")
            };

            string profileResult = null;
            bool completed = false;

            openAIClient.SendChatCompletion(profileMessages, (response, success) => {
                if (success)
                {
                    profileResult = response;
                }
                completed = true;
            });

            yield return new WaitUntil(() => completed);
            onComplete?.Invoke(profileResult);
        }

        private IEnumerator GetRAGContextCoroutine(AIRequest request, string[] categories, System.Action<N8nRAGClient.RAGResponse> onComplete)
        {
            N8nRAGClient.RAGResponse ragResult = null;
            bool completed = false;

            ragClient.QueryRAG(request.userMessage, request.conversationHistory, request.existingProfile, categories,
                (response, success) => {
                    if (success)
                    {
                        ragResult = response;
                    }
                    completed = true;
                });

            yield return new WaitUntil(() => completed);
            onComplete?.Invoke(ragResult);
        }

        private IEnumerator GenerateOpenAIResponseCoroutine(string prompt, List<string> conversationHistory, System.Action<string, int, bool> onComplete)
        {
            var messages = new List<OpenAIClient.Message>
            {
                OpenAIClient.CreateSystemMessage(prompt)
            };

            // Add conversation history
            if (conversationHistory != null)
            {
                foreach (var message in conversationHistory.TakeLast(10)) // Limit to recent history
                {
                    messages.Add(OpenAIClient.CreateUserMessage(message));
                }
            }

            string responseText = null;
            int tokensUsed = 0;
            bool success = false;
            bool completed = false;

            // Subscribe to token usage event temporarily
            System.Action<int> tokenHandler = (tokens) => tokensUsed = tokens;
            OpenAIClient.OnTokensUsed += tokenHandler;

            openAIClient.SendChatCompletion(messages, (response, isSuccess) => {
                responseText = response;
                success = isSuccess;
                completed = true;
            });

            yield return new WaitUntil(() => completed);
            
            // Unsubscribe from token usage event
            OpenAIClient.OnTokensUsed -= tokenHandler;

            onComplete?.Invoke(responseText, tokensUsed, success);
        }

        private string BuildFinalPrompt(AIRequest request, string generatedProfile, N8nRAGClient.RAGResponse ragResponse)
        {
            var promptBuilder = new StringBuilder();
            
            // Base system prompt
            promptBuilder.AppendLine("You are a supportive AI assistant helping users with their concerns and questions.");
            promptBuilder.AppendLine("You should be empathetic, helpful, and provide practical advice when appropriate.");
            promptBuilder.AppendLine();

            // Add user profile information
            if (!string.IsNullOrEmpty(generatedProfile))
            {
                promptBuilder.AppendLine("USER PROFILE INSIGHTS:");
                promptBuilder.AppendLine(generatedProfile);
                promptBuilder.AppendLine();
            }

            // Add RAG context
            if (ragResponse != null && ragResponse.success)
            {
                string ragContext = N8nRAGClient.ExtractContextForPrompt(ragResponse);
                promptBuilder.AppendLine("RELEVANT CONTEXT FROM KNOWLEDGE BASE:");
                promptBuilder.AppendLine(ragContext);
                promptBuilder.AppendLine();
            }

            // Add instructions
            promptBuilder.AppendLine("INSTRUCTIONS:");
            promptBuilder.AppendLine("- Use the user profile insights to personalize your response");
            promptBuilder.AppendLine("- Incorporate relevant context from the knowledge base when applicable");
            promptBuilder.AppendLine("- Be conversational and supportive in your tone");
            promptBuilder.AppendLine("- If the user needs professional help (legal, medical, etc.), recommend seeking appropriate professional assistance");
            promptBuilder.AppendLine("- Keep responses focused and helpful, avoiding overly long explanations");
            promptBuilder.AppendLine();
            
            promptBuilder.AppendLine($"Current user message: {request.userMessage}");

            return promptBuilder.ToString();
        }

        private string GetProfileGenerationPrompt()
        {
            return @"You are a psychological profiling assistant. Based on the user's message and conversation history, generate or update a concise user profile that includes:

1. Personality indicators (communication style, emotional patterns)
2. Current concerns or interests mentioned
3. Support needs or preferences
4. Emotional state indicators

Respond with a JSON-like structured profile that can be used to personalize future interactions. Focus on helpful insights that would improve the support provided to this user.

Keep the profile concise but informative, and update it based on new information from the current message.";
        }

        /// <summary>
        /// Parse profile data from a profile string
        /// </summary>
        public static ProfileData ParseProfile(string profileString)
        {
            if (string.IsNullOrEmpty(profileString))
                return new ProfileData();

            try
            {
                return JsonUtility.FromJson<ProfileData>(profileString);
            }
            catch
            {
                // If JSON parsing fails, create a basic profile from the text
                return new ProfileData
                {
                    personalityType = "Unknown",
                    interests = new string[0],
                    concerns = new string[0],
                    communicationStyle = "Unknown",
                    currentEmotionalState = "Unknown",
                    supportNeeds = new string[0],
                    lastUpdated = DateTime.Now
                };
            }
        }
    }
}