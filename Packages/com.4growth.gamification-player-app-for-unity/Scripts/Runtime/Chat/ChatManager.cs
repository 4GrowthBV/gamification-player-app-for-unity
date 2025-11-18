using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GamificationPlayer.Chat.Services;
using GamificationPlayer.Session;
using Newtonsoft.Json.Linq;

namespace GamificationPlayer.Chat
{
    /// <summary>
    /// ChatManager - Client-side orchestrator for the Gamification Player chat system
    /// 
    /// WHAT IT DOES (Responsibilities):
    /// - Manages chat state (conversation, profile, history, flow type)
    /// - Connects Unity UI events with backend API calls  
    /// - Synchronizes local state with Gamification Player APIs
    /// - Orchestrates predefined and AI message flows
    /// - Emits events for UI layer consumption
    /// 
    /// WHAT IT DOES NOT DO (Non-responsibilities):
    /// - Contains message text, content, or instructions (all come from backend)
    /// - Implements AI/LLM logic (delegates to external AI service)
    /// - Contains agent selection logic (delegates to RAG service)
    /// - Renders UI or controls layouts (purely event-driven)
    /// - Handles authentication (handled by endpoints layer)
    /// 
    /// Flow Orchestration:
    /// 1. Initialization: Check for existing conversation or create new one + profile
    /// 2. Daily Continuation: Detect new days and load appropriate week-based messages (week1_day0, week1_day1, etc.)
    /// 3. Predefined Flow: Handle button clicks, fetch next messages, update profiles via AI
    /// 4. AI Flow: Route messages to agents, fetch instructions, delegate AI generation
    /// 5. Profile Management: Use AI service to generate updated profiles based on conversation
    /// 6. Persistence: Save all messages and AI-generated profile updates to backend
    /// 
    /// Key Behavioral Rules:
    /// - Text input field is always visible (typing switches to AI flow)
    /// - Profile updates occur after every message (user and bot) using AI generation
    /// - ALL content and instructions come from backend (ZERO hardcoded content)
    /// - Profile generation uses AI service with backend instructions and conversation history
    /// - Events are the ONLY way UI interacts with ChatManager
    /// </summary>
    public class ChatManager : MonoBehaviour
    {
        public enum RolePrefix
        {
            user,
            pre_defined,
            user_activity
        }

        public enum MetadataKeys
        {
            ai_character_name, // name of the AI character
            user_name, // name of the user
            start_conversation_date, // date when the conversation started
            context, // context of the conversation
            button_identifier, // identifier of the button clicked
            button_text, // text of the button clicked
            organisation_name // name of the organisation
        }

        public enum Instructions
        {
            agent_memory, // is the instruction for updating the profile of the user based on the conversation
            buddy_router, // is the instruction for routing to the correct AI agent with context based on the conversation
            general // is the general instruction for generating AI responses, is added to all agent instructions
        }

        public enum PredefinedMessageIdentifiers
        {
            week1_day0,
            offtopic
        }

        #region Dependencies
        private GamificationPlayerEndpoints endpoints;
        private ISessionLogData sessionData;
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initialize ChatManager with required dependencies
        /// </summary>
        /// <param name="gamificationPlayerEndpoints">The endpoints instance for API calls</param>
        /// <param name="sessionLogData">The session data instance for data access</param>
        public void Initialize(GamificationPlayerEndpoints gamificationPlayerEndpoints, 
            ISessionLogData sessionLogData)
        {
            endpoints = gamificationPlayerEndpoints;
            sessionData = sessionLogData;
        }

        /// <summary>
        /// Check if ChatManager has been properly initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        public bool IsInitialized()
        {
            return endpoints != null && sessionData != null;
        }

        #endregion

        #region Chat State
        public bool IsLogging = false;
        
        private string currentConversationId;
        private string currentProfileId;
        private List<ChatMessage> conversationHistory = new List<ChatMessage>();

        // Current chat profile data (can be from either Get or Create endpoint)
        private string currentProfile = "";

        // AI Instructions cache for performance
        private Dictionary<string, string> instructionsCache = new Dictionary<string, string>();
        private Dictionary<string, ChatMessage> predefinedMessagesCache = new Dictionary<string, ChatMessage>();
        private bool instructionsLoaded = false;

        [Serializable]
        public class ChatMessage
        {
            public string role; // user or the agent's name or pre_defined or user_activity
            public string message;
            public Button[] buttons;
            public DateTime timestamp;
            public string buttonName; // Store button_name from database for display text resolution
            public Dictionary<string, string> userActivityMetadata; // for user activity messages

            /// <summary>
            /// Constructor with buttons for new chat message creation
            /// </summary>
            public ChatMessage(string role, string message, Button[] buttons = null, string buttonName = "")
            {
                this.role = role;
                this.message = message;
                this.buttons = buttons;
                this.buttonName = buttonName;
                this.timestamp = DateTime.Now;
            }

            /// <summary>
            /// Constructor with explicit timestamp for chat history loading
            /// </summary>
            public ChatMessage(string role, string message, DateTime timestamp, string buttonName = "")
            {
                if(role == RolePrefix.user_activity.ToString())
                {
                    this.userActivityMetadata = message.FromJson<Dictionary<string, string>>();
                }

                this.role = role;
                this.message = message;
                this.timestamp = timestamp;
                this.buttonName = buttonName;
            }

            /// <summary>
            /// Constructor for user activity messages
            /// </summary>
            public ChatMessage(Dictionary<string, string> userActivityMetadata)
            {
                this.role = RolePrefix.user_activity.ToString();
                this.userActivityMetadata = userActivityMetadata;
                this.message = userActivityMetadata.ToJson();
                this.timestamp = DateTime.Now;
            }
        }

        [Serializable]
        public class Button
        {
            public string identifier;
            public string text;

            /// <summary>
            /// Constructor for button
            /// </summary>
            /// <param name="identifier">The unique identifier for the button.</param>
            /// <param name="text">The display text for the button.</param>
            public Button(string identifier, string text = "")
            {
                this.identifier = identifier;
                this.text = text;
            }
        }

        [Serializable]
        public class InitialMetadata
        {
            public string AICharacterName;
            public string userName;
            public DateTime startDate;
            public Dictionary<string, string> additionalMetadata;

            /// <summary>
            /// Constructor for initial metadata that the AI can use for context
            /// </summary>
            /// <param name="userName">The user's name.</param>
            /// <param name="startDate">The start date of the chat.</param>
            /// <param name="additionalMetadata">Any additional metadata as key-value pairs.</param>
            public InitialMetadata(string aICharacterName,
                string userName,
                DateTime startDate,
                Dictionary<string, string> additionalMetadata = null)
            {
                this.AICharacterName = aICharacterName;
                this.userName = userName;
                this.startDate = startDate;
                this.additionalMetadata = additionalMetadata ?? new Dictionary<string, string>();
            }

            /// <summary>
            /// Convert initial metadata to dictionary for API consumption
            /// </summary>
            /// <returns>Dictionary representation of the initial metadata</returns>
            public Dictionary<string, string> ToDictionary()
            {
                var dict = new Dictionary<string, string>()
                {
                    { MetadataKeys.ai_character_name.ToString(), AICharacterName },
                    { MetadataKeys.user_name.ToString(), userName },
                    { MetadataKeys.start_conversation_date.ToString(), startDate.ToString("o") },
                };

                if (additionalMetadata != null)
                {
                    foreach (var kvp in additionalMetadata)
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                }

                return dict;
            }
        }

        [SerializeField]
        public class ResumeConversationMetadata
        {
            public string context = "The user has left the chat and is just returning. Based on their previous messages, we want to resume the conversation smoothly.";
            public Dictionary<string, string> additionalMetadata;

            public ResumeConversationMetadata()
            {
                this.additionalMetadata = new Dictionary<string, string>();
            }

            /// <summary>
            /// Constructor for resume conversation metadata, will be sent when resuming a conversation
            /// </summary>
            /// <param name="context">The context for resuming the conversation.</param>
            /// <param name="additionalMetadata">Any additional metadata as key-value pairs.</param>
            public ResumeConversationMetadata(string context, Dictionary<string, string> additionalMetadata = null)
            {
                this.context = context;
                this.additionalMetadata = additionalMetadata ?? new Dictionary<string, string>();
            }
            
            /// <summary>
            /// Convert resume conversation metadata to dictionary for API consumption
            /// </summary>
            /// <returns>Dictionary representation of the resume conversation metadata</returns>
            public Dictionary<string, string> ToDictionary()
            {
                var dict = new Dictionary<string, string>()
                {
                    { MetadataKeys.context.ToString(), context },
                };

                if (additionalMetadata != null)
                {
                    foreach (var kvp in additionalMetadata)
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                }

                return dict;
            }
        }

        #endregion

        #region Events - UI Layer Communication
        
        /// <summary>
        /// Triggered when a predefined bot message is received with optional buttons
        /// Parameters: (message text, button identifiers array)
        /// UI should display the message and show buttons if provided
        /// </summary>
        public static event Action<ChatMessage> OnMessageReceived;
        
        /// <summary>
        /// Triggered when an AI-generated response is received (free-text flow)
        /// Parameter: AI response text
        /// UI should display this as a bot message without buttons
        /// </summary>
        /// <param name="aiMessage">AI response text</param>
        public static event Action<ChatMessage> OnAIMessageReceived;
        
        /// <summary>
        /// Triggered when any error occurs during chat operations
        /// Parameter: Error message for user display
        /// UI should show error notification to user
        /// </summary>
        /// <param name="errorMessage">Error message for user display</param>
        public static event Action<string> OnErrorOccurred;
        
        /// <summary>
        /// Triggered when chat system is fully initialized and ready
        /// UI should enable chat input and show conversation if resuming
        /// </summary>
        /// <param name="expectNewMessage"></param>
        public static event Action<bool> OnChatInitialized;

        /// <summary>
        /// Triggered when a streaming chunk of AI response is received (optional for real-time updates)
        /// Parameter: Partial AI response chunk
        /// UI can use this to show real-time typing effect
        /// </summary>
        public static event Action<string> OnAIMessageChunkReceived;

        #endregion

        #region Public API

        /// <summary>
        /// Initialize the chat system - reuses existing conversation/profile or creates new ones
        /// </summary>
        /// <param name="aiService">The AI service for profile generation, response generation, and agent selection</param>
        /// <param name="ragService">The RAG service for document retrieval and context management</param>
        /// <param name="initialMetadata">Optional initial metadata that the AI can use for context</param>
        /// <param name="forceNewConversation">Whether to force a new conversation</param>
        public void InitializeChat(IChatAIService aiService,
            IRAGService ragService,
            ResumeConversationMetadata resumeMetadata = null,
            InitialMetadata initialMetadata = null,
            bool forceNewConversation = false)
        {
            if (!IsInitialized())
            {
                OnErrorOccurred?.Invoke("ChatManager not properly initialized with dependencies");
                return;
            }

            if (forceNewConversation)
            {
                currentConversationId = "";
                currentProfileId = "";
                conversationHistory.Clear();
                currentProfile = "";
            }

            StartCoroutine(InitializeChatCoroutine(aiService, ragService, resumeMetadata, initialMetadata, forceNewConversation));
        }

        /// <summary>
        /// Handle user message input - determines predefined vs AI flow
        /// </summary>
        /// <param name="aiService">The AI service for profile generation, response generation, and agent selection</param>
        /// <param name="ragService">The RAG service for document retrieval and context management</param>
        /// <param name="userMessage">The user's text message</param>
        public void HandleUserMessage(IChatAIService aiService,
            IRAGService ragService,
            string userMessage)
        {
            if (!IsInitialized())
            {
                OnErrorOccurred?.Invoke("ChatManager not initialized");
                return;
            }

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                OnErrorOccurred?.Invoke("Message cannot be empty");
                return;
            }

            StartCoroutine(HandleUserMessageCoroutine(aiService, ragService, userMessage));
        }

        /// <summary>
        /// Handle predefined flow button clicks
        /// </summary>
        /// <param name="aiService">The AI service for profile generation, response generation, and agent selection</param>
        /// <param name="buttonIdentifier">The button ID from the backend message</param>
        public void HandleButtonClick(IChatAIService aiService,
            string buttonIdentifier)
        {
            if (!IsInitialized())
            {
                OnErrorOccurred?.Invoke("ChatManager not initialized");
                return;
            }

            StartCoroutine(HandleButtonClickCoroutine(aiService, buttonIdentifier));
        }
        
        /// <summary>
        /// Handle user activity input - tracks specific user actions
        /// </summary>
        /// <param name="aiService">The AI service for profile generation, response generation, and agent selection</param>
        /// <param name="ragService">The RAG service for document retrieval and context management</param>
        /// <param name="userActivityMetadata">Optional metadata dictionary for additional context</param>
        public void HandleUserActivity(IChatAIService aiService,
            IRAGService ragService,
            Dictionary<string, string> userActivityMetadata)
        {
            if (!IsInitialized())
            {
                OnErrorOccurred?.Invoke("ChatManager not initialized");
                return;
            }

            StartCoroutine(HandleUserActivityCoroutine(aiService, ragService, userActivityMetadata));
        }

        /// <summary>
        /// Get the current conversation history
        /// </summary>
        /// <returns>Read-only list of chat messages</returns>
        public List<ChatMessage> GetConversationHistory()
        {
            return new List<ChatMessage>(conversationHistory.OrderBy(m => m.timestamp));
        }

        /// <summary>
        /// Force start a new conversation and profile
        /// </summary>
        /// <param name="aiService">The AI service for profile generation, response generation, and agent selection</param>
        /// <param name="ragService">The RAG service for document retrieval and context management</param>
        /// <param name="initialMetadata">Optional initial metadata that the AI can use for context</param>
        public void ForceNewConversation(IChatAIService aiService,
            IRAGService ragService,
            InitialMetadata initialMetadata = null)
        {
            if (!IsInitialized())
            {
                OnErrorOccurred?.Invoke("ChatManager not properly initialized with dependencies");
                return;
            }

            currentConversationId = "";
            currentProfileId = "";
            conversationHistory.Clear();
            currentProfile = "";

            StartCoroutine(InitializeChatCoroutine(aiService, ragService, null, initialMetadata, true));
        }

        #endregion

        #region Core Flow Implementation

        /// <summary>
        /// Initialize chat conversation and profile with parallel optimization
        /// </summary>
        /// <param name="aiService">The AI service for profile generation, response generation, and agent selection</param>
        /// <param name="ragService">The RAG service for document retrieval and context management</param>
        /// <param name="resumeMetadata">Optional resume metadata for continuing conversation</param>
        /// <param name="initialMetadata">Optional initial metadata that the AI can use for context</param>
        /// <param name="forceNewConversation">Whether to force a new conversation/profile</param>
        private IEnumerator InitializeChatCoroutine(IChatAIService aiService,
            IRAGService ragService,
            ResumeConversationMetadata resumeMetadata = null,
            InitialMetadata initialMetadata = null,
            bool forceNewConversation = false)
        {
            Log($"Initializing chat system... (Force new: {forceNewConversation})");

            // Phase 1: Create/get conversation first (required for profile)
            Log("Phase 1: Creating/getting conversation...");
            yield return StartCoroutine(ExecuteInParallel(CreateOrGetConversation(forceNewConversation),
                LoadPredefinedMessages()));

            if (string.IsNullOrEmpty(currentConversationId))
            {
                OnErrorOccurred?.Invoke("Failed to create or get conversation");
                yield break;
            }

            // Phase 2: Execute profile, history, and instructions loading in parallel
            Log("Phase 2: Loading profile, history, and AI instructions in parallel...");
            yield return StartCoroutine(ExecuteInParallel(
                CreateOrGetChatProfile(forceNewConversation),
                LoadConversationHistory(),
                LoadAIInstructions()
            ));

            if (string.IsNullOrEmpty(currentProfileId))
            {
                OnErrorOccurred?.Invoke("Failed to create or get chat profile");
                yield break;
            }

            Log("Chat system initialized successfully with parallel optimization");

            // Phase 3: Handle daily continuation or initial setup
            Log("Phase 3: Handling daily continuation logic...");
            bool isNewDay = DetectNewDay();

            if (conversationHistory.Count == 0 || forceNewConversation)
            {
                // First time user - start with week1_day0
                Log("First time user detected, loading week1_day0 message");

                OnChatInitialized?.Invoke(true);

                yield return StartCoroutine(SaveInitialMetadata(initialMetadata));
                yield return StartCoroutine(StartPredefinedMessage(aiService, "week1_day0"));
            }
            else if (isNewDay)
            {
                // Existing conversation but new day - load next daily message
                string nextDayIdentifier = GetNextDayIdentifier();
                if (!string.IsNullOrEmpty(nextDayIdentifier))
                {
                    Log($"New day detected, loading: {nextDayIdentifier}");

                    OnChatInitialized?.Invoke(true);

                    yield return StartCoroutine(StartPredefinedMessage(aiService, nextDayIdentifier));
                }
                else
                {
                    Log("New day detected but no daily message available, resuming conversation");

                    OnChatInitialized?.Invoke(false);
                }
            }
            else
            {
                if (resumeMetadata == null)
                {
                    Log("Resuming conversation without resume metadata");

                    OnChatInitialized?.Invoke(false);
                }
                else
                {
                    Log("Resuming conversation with resume metadata");

                    OnChatInitialized?.Invoke(true);

                    yield return StartCoroutine(HandleUserActivityCoroutine(aiService, ragService, resumeMetadata.ToDictionary()));
                }
            }
        }

        /// <summary>
        /// Handle user message through appropriate flow (predefined or AI)
        /// </summary>
        /// <param name="aiService">The AI service instance.</param>
        /// <param name="ragService">The RAG service instance.</param>
        /// <param name="userMessage">The user's text message.</param>
        private IEnumerator HandleUserMessageCoroutine(IChatAIService aiService, IRAGService ragService, string userMessage)
        {
            Log($"Processing user message: {userMessage}");

            var message = new ChatMessage(RolePrefix.user.ToString(), userMessage);

            // Add user message to conversation
            conversationHistory.Add(message);

            // Phase 1: Execute initial operations in parallel
            Log("Phase 1: Starting parallel user message processing...");
            RAGResult ragResult = null;
            string agentName = null;

            yield return StartCoroutine(ExecuteInParallel(
                SaveUserMessageToConversation(message),
                AgentNameAndContextMessageWrapper(aiService, ragService, GetConversationHistory().ToArray(), (name, result) =>
                {
                    ragResult = result;
                    agentName = name;
                })
            ));

            if (ragResult == null || !ragResult.success)
            {
                OnErrorOccurred?.Invoke($"Failed to route message: {ragResult?.errorMessage ?? "Unknown error"}");
                yield break;
            }

            Log($"Phase 1 completed: Agent selected = {agentName}");

            if(agentName == "offtopic")
            {
                Log("Agent routed to 'offtopic', skipping AI response generation.");
                yield return StartCoroutine(StartPredefinedMessage(aiService, "offtopic"));
                yield break;
            }

            // Phase 2: Generate AI response with enhanced instructions (sequential - needs rag result)
            Log("Phase 2: Generating AI response with agent-specific instructions...");
            AIResponseResult aiResult = null;

            yield return StartCoroutine(GenerateAIResponseWithInstructions(aiService,
                agentName,
                ragResult.examples,
                ragResult.knowledge,
                (result) => aiResult = result));

            if (aiResult == null || !aiResult.success)
            {
                OnErrorOccurred?.Invoke($"Failed to generate AI response: {aiResult?.errorMessage ?? "Unknown error"}");
                yield break;
            }

            var agentMessage = new ChatMessage(agentName, aiResult.response);

            // Add AI response to conversation
            conversationHistory.Add(agentMessage);

            // Phase 3: Execute AI response operations in parallel
            Log("Phase 3: Starting parallel AI response processing...");

            yield return StartCoroutine(ExecuteInParallel(
                SaveBotMessageToConversation(agentMessage),
                GenerateUpdatedProfile(aiService)
            ));

            // Notify UI
            OnAIMessageReceived?.Invoke(agentMessage);

            Log($"User message processing completed successfully for agent: {agentName}");
        }

        /// <summary>
        /// Wrapper to get agent name and then fetch context using RAG service
        /// </summary>
        /// <param name="chatAIService">The AI service instance.</param>
        /// <param name="ragService">The RAG service instance.</param>
        /// <param name="conversationContext">The conversation context as an array of messages.</param>
        /// <param name="onComplete">Callback to receive the routing result.</param>
        private IEnumerator AgentNameAndContextMessageWrapper(IChatAIService chatAIService,
            IRAGService ragService,
            ChatMessage[] conversationContext,
            Action<string, RAGResult> onComplete)
        {
            var agentNameInstruction = GetInstruction(Instructions.buddy_router.ToString());

            string agentName = "";
            string fewShotPrompt = "";
            string dataBankPrompt = "";

            yield return StartCoroutine(chatAIService.GetAIAgentNameAndPrompts(conversationContext,
                agentNameInstruction,
                (result) =>
            {
                agentName = result.agentName;
                fewShotPrompt = result.fewShotPrompt;
                dataBankPrompt = result.dataBankPrompt;
            }));

            if(agentName == "offtopic")
            {
                onComplete("offtopic", new RAGResult("offtopic", "offtopic"));
                yield break;
            }

            yield return StartCoroutine(ragService.GetContextForUserMessage(agentName,
                fewShotPrompt,
                dataBankPrompt,
                conversationContext,
                (result) =>
            {
                onComplete(agentName, result);
            }));
        }

        /// <summary>
        /// Handle predefined flow button clicks
        /// </summary>
        /// <param name="aiService">The AI service instance.</param>
        /// <param name="buttonIdentifier">The button ID from the backend message.</param>
        private IEnumerator HandleButtonClickCoroutine(IChatAIService aiService, string buttonIdentifier)
        {
            Log($"Processing button click: {buttonIdentifier}");

            if (!predefinedMessagesCache.TryGetValue(buttonIdentifier, out var dtoMessage))
            {
                OnErrorOccurred?.Invoke($"Button identifier not found in predefined messages: {buttonIdentifier}");
                yield break;
            }

            var message = new ChatMessage(new Dictionary<string, string>
            {
                { MetadataKeys.context.ToString(), "User pressed on a button of the previous message, this generated a new message" },
                { MetadataKeys.button_identifier.ToString(), buttonIdentifier },
                { MetadataKeys.button_text.ToString(), dtoMessage.buttonName },
                { MetadataKeys.ai_character_name.ToString(), GetAICharacterName() },
                { MetadataKeys.user_name.ToString(), GetUserName() },
                { MetadataKeys.start_conversation_date.ToString(), GetUserConversationDate().ToString("o") }
            });

            // Add button click as user message
            conversationHistory.Add(message);

            // Phase 1: Execute user action processing in parallel
            Log("Starting parallel button click processing...");

            yield return StartCoroutine(SaveUserMessageToConversation(message));

            // Phase 2: Load next predefined message (sequential - depends on button identifier)
            Log($"Loading predefined message for button: {buttonIdentifier}");

            yield return StartCoroutine(ExecuteInParallel(
                StartPredefinedMessage(aiService, buttonIdentifier),
                GenerateUpdatedProfile(aiService)
            ));

            Log($"Button click processing completed for: {buttonIdentifier}");
        }

        /// <summary>
        /// Handle user activity input - tracks specific user actions
        /// </summary>
        /// <param name="aiService">The AI service instance.</param>
        /// <param name="ragService">The RAG service instance.</param>
        /// <param name="activityMetadata">Optional metadata dictionary for additional context.</param>
        private IEnumerator HandleUserActivityCoroutine(IChatAIService aiService,
            IRAGService ragService,
            Dictionary<string, string> activityMetadata)
        {
            var message = new ChatMessage(activityMetadata);

            if (!message.userActivityMetadata.ContainsKey(MetadataKeys.start_conversation_date.ToString()))
            {
                message.userActivityMetadata[MetadataKeys.start_conversation_date.ToString()] = GetUserConversationDate().ToString("o");
            }

            if (!message.userActivityMetadata.ContainsKey(MetadataKeys.ai_character_name.ToString()))
            {
                message.userActivityMetadata[MetadataKeys.ai_character_name.ToString()] = GetAICharacterName();
            }

            if (!message.userActivityMetadata.ContainsKey(MetadataKeys.user_name.ToString()))
            {
                message.userActivityMetadata[MetadataKeys.user_name.ToString()] = GetUserName();
            }

            message.message = message.userActivityMetadata.ToJson();

            Log($"Processing user activity: {message.message}");
            
            // Add activity as user message
            conversationHistory.Add(message);

            // Phase 1: Execute user activity processing in parallel
            Log("Starting parallel user activity processing...");
            RAGResult ragResult = null;
            string agentName = null;

            yield return StartCoroutine(ExecuteInParallel(
                SaveUserMessageToConversation(message),
                AgentNameAndContextMessageWrapper(aiService, ragService, GetConversationHistory().ToArray(), (name, result) =>
                {
                    ragResult = result;
                    agentName = name;
                })
            ));

            if (ragResult == null || !ragResult.success)
            {
                OnErrorOccurred?.Invoke($"Failed to route activity message: {ragResult?.errorMessage ?? "Unknown error"}");
                yield break;
            }

            Log($"Phase 1 completed: Agent selected = {agentName}");

            if(agentName == "offtopic")
            {
                Log("Agent routed to 'offtopic', skipping AI response generation.");
                yield break;
            }

            // Phase 2: Generate AI response with enhanced instructions (sequential - needs rag result)
            Log("Phase 2: Generating AI response with agent-specific instructions...");
            AIResponseResult aiResult = null;

            yield return StartCoroutine(GenerateAIResponseWithInstructions(aiService,
                agentName,
                ragResult.examples,
                ragResult.knowledge,
                (result) => aiResult = result));

            if (aiResult == null || !aiResult.success)
            {
                OnErrorOccurred?.Invoke($"Failed to generate AI response: {aiResult?.errorMessage ?? "Unknown error"}");
                yield break;
            }

            var agentMessage = new ChatMessage(agentName, aiResult.response);

            // Add AI response to conversation
            conversationHistory.Add(agentMessage);

            // Phase 3: Execute AI response operations in parallel
            Log("Phase 3: Starting parallel AI response processing...");

            yield return StartCoroutine(ExecuteInParallel(
                SaveBotMessageToConversation(agentMessage),
                GenerateUpdatedProfile(aiService)
            ));

            // Notify UI
            OnAIMessageReceived?.Invoke(agentMessage);

            Log($"User activity processing completed successfully for agent: {agentName}");
        }

        /// <summary>
        /// Start predefined message flow for daily continuation or button clicks
        /// </summary>
        /// <param name="aiService">The AI service instance.</param>
        /// <param name="identifier">The predefined message identifier.</param>
        private IEnumerator StartPredefinedMessage(IChatAIService aiService, string identifier)
        {
            Log($"Loading predefined message: {identifier}");

            // start with week1_day0 as fallback
            var role = RolePrefix.pre_defined.ToString() + "-" + "week1_day0";
            var messageText = "Welcome to the chat!"; // ultimate fallback
            var buttons = new Button[] { };

            if (predefinedMessagesCache.TryGetValue("week1_day0", out var cachedMessage))
            {
                messageText = cachedMessage.message;
                buttons = cachedMessage.buttons;
            }
            else
            {
                LogWarning($"No cached message found for week1_day0, using fallback.");

                var fallback = predefinedMessagesCache.Values.FirstOrDefault();

                if (fallback != null)
                {
                    messageText = fallback.message;
                    buttons = fallback.buttons;
                }
            }

            if (predefinedMessagesCache.TryGetValue(identifier, out cachedMessage))
            {
                messageText = cachedMessage.message;
                buttons = cachedMessage.buttons;
                role = RolePrefix.pre_defined.ToString() + "-" + identifier;
            }

            var message = new ChatMessage(role, messageText, buttons);

            conversationHistory.Add(message);

            // Execute predefined message processing in parallel
            Log("Processing predefined message in parallel...");

            yield return StartCoroutine(ExecuteInParallel(
                SaveBotMessageToConversation(message),
                GenerateUpdatedProfile(aiService)
            ));

            // Notify UI
            OnMessageReceived?.Invoke(message);
        }

        /// <summary>
        /// Save user message to backend conversation
        /// </summary>
        /// <param name="chatMessage">The user chat message to save.</param>
        private IEnumerator SaveUserMessageToConversation(ChatMessage chatMessage)
        {
            if (string.IsNullOrEmpty(currentConversationId))
            {
                LogWarning("No conversation ID available for saving user message");
                yield break;
            }

            bool messageSaved = false;
            yield return StartCoroutine(endpoints.CoCreateChatConversationMessage(
                chatMessage.role,
                chatMessage.message,
                Guid.Parse(currentConversationId),
                (result, dto) =>
                {
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        Log("User message saved successfully");
                    }
                    else
                    {
                        LogError($"Failed to save user message: {result}");
                    }
                    messageSaved = true;
                }));

            yield return new WaitUntil(() => messageSaved);
        }

        /// <summary>
        /// Save bot message to backend conversation
        /// </summary>
        /// <param name="chatMessage">The bot chat message to save.</param>
        private IEnumerator SaveBotMessageToConversation(ChatMessage chatMessage)
        {
            if (string.IsNullOrEmpty(currentConversationId))
            {
                LogWarning("No conversation ID available for saving bot message");
                yield break;
            }

            bool messageSaved = false;
            yield return StartCoroutine(endpoints.CoCreateChatConversationMessage(
                chatMessage.role,
                chatMessage.message,
                Guid.Parse(currentConversationId),
                (result, dto) =>
                {
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        Log("Bot message saved successfully");
                    }
                    else
                    {
                        LogError($"Failed to save bot message: {result}");
                    }
                    messageSaved = true;
                }));

            yield return new WaitUntil(() => messageSaved);
        }

        /// <summary>
        /// Create or get existing conversation
        /// </summary>
        /// <param name="forceNew">Whether to force a new conversation creation</param>
        private IEnumerator CreateOrGetConversation(bool forceNew = false)
        {
            if (!forceNew && !string.IsNullOrEmpty(currentConversationId))
            {
                // Check if conversation still exists
                bool conversationExists = false;
                yield return StartCoroutine(endpoints.CoGetChatConversation(Guid.Parse(currentConversationId), (result, dto) =>
                {
                    conversationExists = result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null;

                    if (conversationExists)
                    {
                        currentProfileId = dto.included.Where(i => i.Type == "chat_profile").Select(i => i.id).FirstOrDefault();
                    }
                }));

                if (conversationExists)
                {
                    Log($"Using existing conversation: {currentConversationId}");
                    yield break;
                }
            }
            else if (!forceNew && string.IsNullOrEmpty(currentConversationId))
            {
                // Check if there is an existing conversation to reuse
                bool conversationExists = false;
                yield return StartCoroutine(endpoints.CoGetChatConversations((result, dto) =>
                {
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        var conversation = dto.data.OrderBy(c => c.attributes.CreatedAt).LastOrDefault();

                        currentConversationId = conversation?.id;

                        conversationExists = !string.IsNullOrEmpty(currentConversationId);

                        if (conversationExists && conversation.relationships != null)
                        {
                            currentProfileId = conversation.relationships
                                .Where(kvp => kvp.Key == "chat_profile")
                                .Select(kvp => GetId(kvp.Value)).FirstOrDefault();
                        }
                    }
                }));

                if (conversationExists)
                {
                    Log($"Using existing conversation: {currentConversationId} with profile: {currentProfileId}");
                    yield break;
                }
            }

            // Create new conversation
            bool conversationCreated = false;
            yield return StartCoroutine(endpoints.CoCreateChatConversation((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    currentConversationId = dto.data.id;
                    Log($"Created new conversation: {currentConversationId}");
                }
                else
                {
                    LogError($"Failed to create conversation: {result}");
                }
                conversationCreated = true;
            }));

            yield return new WaitUntil(() => conversationCreated);
        }

        private string GetId(object obj)
        {
            JObject jObj = obj switch
            {
                string s => JObject.Parse(s),
                JObject j => j,
                _ => JObject.FromObject(obj)
            };

            var idToken = jObj.SelectToken("data.id") ?? jObj["id"];
            return idToken?.ToString();
        }

        /// <summary>
        /// Create or get existing chat profile
        /// </summary>
        /// <param name="forceNew">Whether to force a new profile creation</param>
        private IEnumerator CreateOrGetChatProfile(bool forceNew = false)
        {
            if (!forceNew && !string.IsNullOrEmpty(currentProfileId))
            {
                // Try to get existing profile using conversation ID
                bool profileChecked = false;
                bool profileExists = false;

                yield return StartCoroutine(endpoints.CoGetChatProfile(Guid.Parse(currentProfileId), (result, dto) =>
                {
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                    {
                        currentProfile = dto.data.attributes.profile;
                        currentProfileId = dto.data.id;
                        profileExists = true;
                        Log($"Found existing profile: {currentProfileId}");
                    }
                    else
                    {
                        Log($"No existing profile found {currentProfileId}");
                    }
                    profileChecked = true;
                }));

                yield return new WaitUntil(() => profileChecked);

                if (profileExists)
                {
                    yield break;
                }
            }

            // Create new profile if none exists or forced
            bool profileCreated = false;
            yield return StartCoroutine(endpoints.CoCreateChatProfile("no profile", Guid.Parse(currentConversationId), (result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    currentProfile = dto.data.attributes.profile;
                    currentProfileId = dto.data.id;
                    Log($"Created new profile: {currentProfileId}");
                }
                else
                {
                    LogError($"Failed to create profile: {result}");
                }
                profileCreated = true;
            }));

            yield return new WaitUntil(() => profileCreated);
        }
        
        /// <summary>
        /// Save initial metadata as user activity message
        /// </summary>
        /// <param name="userActivityMetadata">The initial metadata to save.</param>
        private IEnumerator SaveInitialMetadata(InitialMetadata userActivityMetadata = null)
        {
            Log("Saving initial metadata as user activity message");
            if (userActivityMetadata == null)
            {
                Log("No initial metadata provided, fetching from session data");
                sessionData.TryGetLatest<UserName>(out var userName);
                sessionData.TryGetLatest<OrganisationName>(out var organisationName);

                userActivityMetadata = new InitialMetadata("AI Chat Bot",
                    string.IsNullOrEmpty(userName) ? "unknown" : userName,
                    DateTime.Now,
                    new Dictionary<string, string>
                    {
                        { MetadataKeys.organisation_name.ToString(), string.IsNullOrEmpty(organisationName) ? "unknown" : organisationName }
                    }
                );
            }

            var message = new ChatMessage(userActivityMetadata.ToDictionary());

            conversationHistory.Add(message);

            yield return StartCoroutine(SaveUserMessageToConversation(message));
        }

        /// <summary>
        /// Load conversation history from backend
        /// </summary>
        private IEnumerator LoadConversationHistory()
        {
            if (string.IsNullOrEmpty(currentConversationId))
            {
                LogWarning("No conversation ID available for loading history");
                yield break;
            }

            bool historyLoaded = false;
            yield return StartCoroutine(endpoints.CoGetChatConversationMessages((result, dto) =>
            {

                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    conversationHistory.Clear();
                    foreach (var message in dto.data)
                    {
                        conversationHistory.Add(new ChatMessage(
                            message.attributes.role,
                            message.attributes.message,
                            message.attributes.CreatedAt
                        ));
                    }
                    Log($"Loaded {conversationHistory.Count} messages from conversation history");
                }
                else
                {
                    LogError($"Failed to load conversation history: {result}");
                }
                historyLoaded = true;
            }, Guid.Parse(currentConversationId)));

            yield return new WaitUntil(() => historyLoaded);

            // we have to check if latest message is a predefined message so we can load the buttons
            if (GetConversationHistory().Count > 0)
            {
                var lastMessage = GetConversationHistory()
                    .Where(m => m.role != RolePrefix.user_activity.ToString())
                    .OrderBy(m => m.timestamp).Last();
                    
                if (lastMessage.role.Contains(RolePrefix.pre_defined.ToString()))
                {
                    var identifier = lastMessage.role.Split('-').Last(); // extract identifier from role

                    predefinedMessagesCache.TryGetValue(identifier, out var dtoMessage);
                    
                    lastMessage.buttons = dtoMessage.buttons
                        .Select(b => GetPredefinedButton(b.identifier))
                        .Where(button => button != null) // Filter out null buttons (from "none" values)
                        .ToArray();
                    Log($"Loaded buttons for last predefined message");
                }
            }

            yield return null;
        }

        /// <summary>
        /// Detect if this is a new day since last chat
        /// </summary>
        private bool DetectNewDay()
        {
            var lastChatDate = conversationHistory.Any() ? conversationHistory.Max(m => m.timestamp) : DateTime.MaxValue;
            Log($"Detecting new day. Last chat date: {lastChatDate}, Today: {DateTime.Now.Date}, is new day: {lastChatDate.Date < DateTime.Now.Date}");
            return lastChatDate.Date < DateTime.Now.Date;
        }

        /// <summary>
        /// Get the second day identifier. This one is unique, day one is always week1_day0, day two is based of user start date
        /// The days after that are sequentially after the last predefined message in the conversation history.
        /// </summary>
        private string GetNextDayIdentifier()
        {
            var userStartDate = GetUserConversationDate();

            // get days difference
            var daysDifference = (DateTime.Now.Date - userStartDate.Date).Days;

            // get week/day based on days difference
            var week = daysDifference / 7 + 1;
            var day = daysDifference % 7;

            var weekDayMessages = predefinedMessagesCache.Keys
                .Where(k => k.StartsWith("week") && k.Contains("_day") && !k.Contains("_a") && !k.Contains("_b") && !k.Contains("_c"))
                .ToDictionary(k => ExtractWeekDay(k), k => k);

            // get closest matching predefined message
            var closestMessage = weekDayMessages
                .OrderBy(kv => Math.Abs(kv.Key - (week * 100 + day)))
                .FirstOrDefault();

            return closestMessage.Value;
        }

        private string GetAICharacterName()
        {
            var firstMessage = conversationHistory
                .Where(m => m.role == RolePrefix.user_activity.ToString())
                .FirstOrDefault();

            if (firstMessage != null)
            {
                // Try to parse from initial metadata message
                try
                {
                    var metadata = firstMessage.message.FromJson<Dictionary<string, string>>();
                    if (metadata != null &&
                        metadata.TryGetValue(MetadataKeys.ai_character_name.ToString(), out var aiCharacterName))
                    {
                        return aiCharacterName;
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to parse user start date from initial metadata: {ex.Message}");
                }
            }

            return "unknown_character";
        }

        private string GetUserName()
        {
            var firstMessage = conversationHistory
                .Where(m => m.role == RolePrefix.user_activity.ToString())
                .FirstOrDefault();

            if (firstMessage != null)
            {
                // Try to parse from initial metadata message
                try
                {
                    var metadata = firstMessage.message.FromJson<Dictionary<string, string>>();
                    if (metadata != null &&
                        metadata.TryGetValue(MetadataKeys.user_name.ToString(), out var userName))
                    {
                        return userName;
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to parse user name from initial metadata: {ex.Message}");
                }
            }

            return "unknown_user";
        }

        private DateTime GetUserConversationDate()
        {
            var firstMessage = conversationHistory
                .Where(m => m.role == RolePrefix.user_activity.ToString())
                .FirstOrDefault();

            if (firstMessage != null)
            {
                // Try to parse from initial metadata message
                try
                {
                    var metadata = firstMessage.message.FromJson<Dictionary<string, string>>();
                    if (metadata != null &&
                        metadata.TryGetValue(MetadataKeys.start_conversation_date.ToString(), out var startDateStr) &&
                        DateTime.TryParse(startDateStr, out var startDate))
                    {
                        return startDate;
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to parse user start date from initial metadata: {ex.Message}");
                }
            }

            firstMessage = conversationHistory
                .OrderBy(m => m.timestamp)
                .FirstOrDefault();

            // Fallback to first message timestamp if no start date found
            return firstMessage?.timestamp ?? DateTime.MinValue;
        }

        /// <summary>
        /// Extract week and day numbers for sorting (e.g., "week2_day3" becomes 203)
        /// </summary>
        private int ExtractWeekDay(string identifier)
        {
            // Convert "week2_day3" to sortable number (e.g., 203 for week 2, day 3)
            var parts = identifier.Split('_');
            if (parts.Length >= 2)
            {
                var weekStr = parts[0].Replace("week", "");
                var dayStr = parts[1].Replace("day", "");

                if (int.TryParse(weekStr, out int week) && int.TryParse(dayStr, out int day))
                {
                    return week * 100 + day; // week2_day3 becomes 203
                }
            }
            return 0;
        }

        /// <summary>
        /// Generate updated profile using dedicated profile generation method
        /// </summary>
        /// <param name="aiService">The AI service instance.</param>
        private IEnumerator GenerateUpdatedProfile(IChatAIService aiService)
        {
            Log($"Generating updated profile using dedicated profile method for message");
            
            // Get cached profile generation instructions
            string profileInstructions = GetInstruction(Instructions.agent_memory.ToString());
            
            if (string.IsNullOrEmpty(profileInstructions))
            {
                LogWarning("No profile generator instructions available, skipping profile update");
                yield break;
            }

            // Use dedicated profile generation method
            AIResponseResult profileResult = null;
            
            yield return StartCoroutine(aiService.GenerateProfile(
                currentProfile,
                GetConversationHistory().ToArray(),
                profileInstructions,
                (result) => profileResult = result));

            if (profileResult != null && profileResult.success)
            {
                // Update current profile with AI-generated result
                string newProfile = profileResult.response;
                currentProfile = newProfile;
                
                // Save updated profile to backend
                yield return StartCoroutine(SaveProfileToBackend(newProfile));
                
                Log($"Profile updated successfully using dedicated profile method: {newProfile.Substring(0, Math.Min(100, newProfile.Length))}...");
            }
            else
            {
                LogWarning($"Failed to generate profile update: {profileResult?.errorMessage ?? "Unknown error"}");
                // Continue anyway, this is not critical
            }
        }

        /// <summary>
        /// Save updated profile to backend
        /// </summary>
        private IEnumerator SaveProfileToBackend(string profileContent)
        {
            if (string.IsNullOrEmpty(currentProfileId))
            {
                LogWarning("No profile ID available for saving profile");
                yield break;
            }

            bool profileSaved = false;
            yield return StartCoroutine(endpoints.CoUpdateChatProfile(Guid.Parse(currentProfileId), profileContent, (result, dto) =>
            {

                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Log("Profile saved to backend successfully");
                }
                else
                {
                    LogError($"Failed to save profile to backend: {result}");
                }
                
                profileSaved = true;
            }));

            yield return new WaitUntil(() => profileSaved);
        }

        #endregion

        private void LogError(string message)
        {
            if (IsLogging)
            {
                Debug.LogError(message);
            }
        }

        private void Log(string message)
        {
            if (IsLogging)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (IsLogging)
            {
                Debug.LogWarning(message);
            }
        }

        #region Predefined Messages Management

        private IEnumerator LoadPredefinedMessages()
        {
            if (predefinedMessagesCache.Count > 0)
            {
                Log("Predefined messages already loaded, skipping");
                yield break;
            }

            Log("Loading predefined messages from backend...");

            bool loadCompleted = false;
            string errorMessage = "";

            yield return StartCoroutine(endpoints.CoGetChatPredefinedMessages((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    predefinedMessagesCache.Clear();

                    foreach (var message in dto.data)
                    {
                        if (!string.IsNullOrEmpty(message.attributes?.identifier) &&
                            !string.IsNullOrEmpty(message.attributes?.content))
                        {
                            // Filter out "none" buttons during loading
                            var filteredButtons = message.attributes.buttons
                                .Where(b => b != "none")
                                .Select(b => new Button(b, "")) // Will be resolved later
                                .ToArray();

                            predefinedMessagesCache[message.attributes.identifier] = new ChatMessage(
                                RolePrefix.pre_defined.ToString() + "-" + message.attributes.identifier,
                                message.attributes.content,
                                filteredButtons,
                                message.attributes.button_name // Store the button_name
                            );
                        }
                    }

                    // Now resolve button display text using button_name
                    foreach (var msg in predefinedMessagesCache.Values)
                    {
                        for (int i = 0; i < msg.buttons.Length; i++)
                        {
                            var button = msg.buttons[i];
                            if (predefinedMessagesCache.TryGetValue(button.identifier, out ChatMessage targetMessage))
                            {
                                // Use the target message's button_name as display text
                                msg.buttons[i] = new Button(button.identifier, targetMessage.buttonName);
                            }
                        }
                    }

                    Log($"Successfully loaded {predefinedMessagesCache.Count} predefined messages");
                }
                else
                {
                    errorMessage = $"Failed to load predefined messages: {result}";
                    LogError(errorMessage);
                }

                loadCompleted = true;
            }));

            yield return new WaitUntil(() => loadCompleted);
        }

        private Button GetPredefinedButton(string identifier)
        {
            // Skip "none" values
            if (identifier == "none")
                return null;

            if (predefinedMessagesCache.TryGetValue(identifier, out ChatMessage cachedMessage))
            {
                return new Button(identifier, cachedMessage.buttonName);
            }

            return new Button(identifier, identifier); // fallback
        }

        #endregion

        #region AI Instructions Management

        /// <summary>
        /// Load all AI instructions from backend and cache them for performance
        /// </summary>
        private IEnumerator LoadAIInstructions()
        {
            if (instructionsLoaded)
            {
                Log("AI instructions already loaded, skipping");
                yield break;
            }

            Log("Loading AI instructions from backend...");
            
            bool loadCompleted = false;
            string errorMessage = "";

            yield return StartCoroutine(endpoints.CoGetChatInstructions((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    instructionsCache.Clear();

                    foreach (var instruction in dto.data)
                    {
                        if (!string.IsNullOrEmpty(instruction.attributes?.identifier) &&
                            !string.IsNullOrEmpty(instruction.attributes?.instruction))
                        {
                            instructionsCache[instruction.attributes.identifier] = instruction.attributes.instruction;
                        }
                    }

                    instructionsLoaded = true;
                    Log($"Successfully loaded {instructionsCache.Count} AI instructions");
                }
                else
                {
                    errorMessage = $"Failed to load AI instructions: {result}";
                    LogError(errorMessage);
                }
                loadCompleted = true;
            }));

            yield return new WaitUntil(() => loadCompleted);
        }

        /// <summary>
        /// Get specific instruction by identifier from cache
        /// </summary>
        /// <param name="identifier">The instruction identifier (e.g., "profile_generator", agent names)</param>
        /// <returns>The instruction text or empty string if not found</returns>
        private string GetInstruction(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                LogWarning("GetInstruction called with null or empty identifier");
                return "";
            }

            string instruction;
            if (!instructionsCache.TryGetValue(identifier, out instruction))
            {
                LogWarning($"No instruction found for identifier: {identifier}");
            }

            if (identifier == Instructions.general.ToString() ||
                identifier != Instructions.agent_memory.ToString() ||
                identifier != Instructions.buddy_router.ToString())
            {
                return instruction;
            }

            if (instructionsCache.TryGetValue(Instructions.general.ToString(), out string generalInstruction))
            {
                // add general instruction to the beginning
                instruction = generalInstruction + "\n\n" + instruction;
            }

            return instruction;
        }

        /// <summary>
        /// Generate AI response with agent-specific instructions, examples, and knowledge
        /// </summary>
        /// <param name="aiService">The AI service instance.</param>
        /// <param name="agent">The agent name.</param>
        /// <param name="examples">The few-shot examples.</param>
        /// <param name="knowledge">The knowledge context.</param>
        /// <param name="onComplete">Callback when response is complete.</param>
        /// <returns>Coroutine for AI response generation.</returns>
        private IEnumerator GenerateAIResponseWithInstructions(IChatAIService aiService,
            string agent,
            string examples,
            string knowledge,
            Action<AIResponseResult> onComplete)
        {
            // Get agent-specific instruction
            string agentInstruction = GetInstruction(agent);
            
            if (string.IsNullOrEmpty(agentInstruction))
            {
                LogWarning($"No specific instruction found for agent: {agent}, using fallback instruction");
                agentInstruction = $"You are {agent}. Respond helpfully to the user's message."; // Basic fallback
            }
            
            Log($"Generating AI response for agent '{agent}' with instruction length: {agentInstruction.Length}");
            
            yield return StartCoroutine(aiService.GenerateResponse(
                agentInstruction,
                examples,
                knowledge,
                currentProfile,
                GetConversationHistory().ToArray(), // Add conversation history
                OnStreamChunk, // Handle streaming chunks
                onComplete
            ));
        }

        /// <summary>
        /// Handle streaming response chunks from AI service
        /// </summary>
        /// <param name="chunk">Partial response chunk</param>
        private void OnStreamChunk(string chunk)
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                Log($"Received AI stream chunk: {chunk.Substring(0, Math.Min(50, chunk.Length))}...");
                // Emit streaming event for real-time UI updates
                OnAIMessageChunkReceived?.Invoke(chunk);
            }
        }

        #endregion

        #region Parallel Execution Utilities

        /// <summary>
        /// Execute multiple coroutines in parallel and wait for all to complete
        /// </summary>
        /// <param name="coroutines">Array of coroutines to execute in parallel</param>
        /// <returns>Coroutine that completes when all input coroutines finish</returns>
        private IEnumerator ExecuteInParallel(params IEnumerator[] coroutines)
        {
            if (coroutines == null || coroutines.Length == 0)
            {
                LogWarning("ExecuteInParallel called with no coroutines");
                yield break;
            }

            // Start all coroutines
            Coroutine[] runningCoroutines = new Coroutine[coroutines.Length];
            bool[] completionFlags = new bool[coroutines.Length];

            for (int i = 0; i < coroutines.Length; i++)
            {
                int index = i; // Capture for closure
                runningCoroutines[i] = StartCoroutine(TrackCoroutineCompletion(coroutines[i], index, completionFlags));
            }

            // Wait for all to complete
            yield return new WaitUntil(() => AllCoroutinesComplete(completionFlags));

            Log($"ExecuteInParallel: All {coroutines.Length} coroutines completed");
        }

        /// <summary>
        /// Helper to track individual coroutine completion
        /// </summary>
        private IEnumerator TrackCoroutineCompletion(IEnumerator coroutine, int index, bool[] completionFlags)
        {
            yield return StartCoroutine(coroutine);
            completionFlags[index] = true;
        }

        /// <summary>
        /// Check if all coroutines in the parallel execution have completed
        /// </summary>
        private bool AllCoroutinesComplete(bool[] completionFlags)
        {
            for (int i = 0; i < completionFlags.Length; i++)
            {
                if (!completionFlags[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Utility class for parallel task execution with result capture
        /// </summary>
        private class ParallelTask<T>
        {
            public IEnumerator coroutine;
            public T result;
            public string taskName;

            public ParallelTask(IEnumerator coroutine, string taskName = "")
            {
                this.coroutine = coroutine;
                this.taskName = taskName;
            }
        }

        /// <summary>
        /// Convenience method for executing 2 coroutines in parallel
        /// </summary>
        private IEnumerator ExecuteInParallel(IEnumerator coroutine1, IEnumerator coroutine2)
        {
            yield return StartCoroutine(ExecuteInParallel(new IEnumerator[] { coroutine1, coroutine2 }));
        }

        /// <summary>
        /// Convenience method for executing 3 coroutines in parallel
        /// </summary>
        private IEnumerator ExecuteInParallel(IEnumerator coroutine1, IEnumerator coroutine2, IEnumerator coroutine3)
        {
            yield return StartCoroutine(ExecuteInParallel(new IEnumerator[] { coroutine1, coroutine2, coroutine3 }));
        }

        #endregion
    }
}