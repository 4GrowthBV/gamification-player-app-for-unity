using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GamificationPlayer.Chat.Services;

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
    /// - Contains agent selection logic (delegates to Router service)
    /// - Renders UI or controls layouts (purely event-driven)
    /// - Handles authentication (handled by endpoints layer)
    /// 
    /// Flow Orchestration:
    /// 1. Initialization: Check for existing conversation or create new one + profile
    /// 2. Daily Continuation: Detect new days and load appropriate day_X messages
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
        public enum Role
        {
            user,
            pre_defined,
            user_button
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
        /// <param name="chatRouterService">Router service for agent selection</param>
        /// <param name="chatAIService">AI service for response generation and profile management</param>
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
        [SerializeField] private bool isLogging = true;
        [SerializeField] private string currentConversationId;
        [SerializeField] private string currentProfileId;
        [SerializeField] private List<ChatMessage> conversationHistory = new List<ChatMessage>();
        [SerializeField] private DateTime lastChatDate = DateTime.MinValue;

        // Current chat profile data (can be from either Get or Create endpoint)
        private string currentProfile = "";
        
        // AI Instructions cache for performance
        private Dictionary<string, string> instructionsCache = new Dictionary<string, string>();
        private bool instructionsLoaded = false;
        
        [Serializable]
        public class ChatMessage
        {
            public string role; // user or the agent's name or pre_defined or user_button
            public string message;
            public DateTime timestamp;
            
            public ChatMessage(string role, string message)
            {
                this.role = role;
                this.message = message;
                this.timestamp = DateTime.Now;
            }
        }
        #endregion

        #region Events - UI Layer Communication
        
        /// <summary>
        /// Triggered when a predefined bot message is received with optional buttons
        /// Parameters: (message text, button identifiers array)
        /// UI should display the message and show buttons if provided
        /// </summary>
        public static event Action<string, string[]> OnMessageReceived;
        
        /// <summary>
        /// Triggered when an AI-generated response is received (free-text flow)
        /// Parameter: AI response text
        /// UI should display this as a bot message without buttons
        /// </summary>
        public static event Action<string> OnAIMessageReceived;
        
        /// <summary>
        /// Triggered when any error occurs during chat operations
        /// Parameter: Error message for user display
        /// UI should show error notification to user
        /// </summary>
        public static event Action<string> OnErrorOccurred;
        
        /// <summary>
        /// Triggered when chat system is fully initialized and ready
        /// UI should enable chat input and show conversation if resuming
        /// </summary>
        public static event Action OnChatInitialized;
        
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
        public void InitializeChat(IChatAIService aiService)
        {
            if (!IsInitialized())
            {
                OnErrorOccurred?.Invoke("ChatManager not properly initialized with dependencies");
                return;
            }
            
            StartCoroutine(InitializeChatCoroutine(aiService));
        }
        
        /// <summary>
        /// Handle user message input - determines predefined vs AI flow
        /// </summary>
        /// <param name="userMessage">The user's text message</param>
        public void HandleUserMessage(IChatAIService aiService, IChatRouterService routerService, string userMessage)
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
            
            StartCoroutine(HandleUserMessageCoroutine(aiService, routerService, userMessage));
        }
        
        /// <summary>
        /// Handle predefined flow button clicks
        /// </summary>
        /// <param name="buttonIdentifier">The button ID from the backend message</param>
        public void HandleButtonClick(IChatAIService aiService, string buttonIdentifier)
        {
            if (!IsInitialized())
            {
                OnErrorOccurred?.Invoke("ChatManager not initialized");
                return;
            }

            StartCoroutine(HandleButtonClickCoroutine(aiService, buttonIdentifier));
        }
        
        #endregion

        #region Core Flow Implementation

        /// <summary>
        /// Initialize chat conversation and profile with parallel optimization
        /// </summary>
        private IEnumerator InitializeChatCoroutine(IChatAIService aiService, bool forceNewConversation = false)
        {
            Log($"Initializing chat system... (Force new: {forceNewConversation})");
            
            // Phase 1: Create/get conversation first (required for profile)
            Log("Phase 1: Creating/getting conversation...");
            yield return StartCoroutine(CreateOrGetConversation(forceNewConversation));
            
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

            // Phase 3: Handle daily continuation or initial setup
            Log("Phase 3: Handling daily continuation logic...");
            bool isNewDay = DetectNewDay();
            
            if (conversationHistory.Count == 0 || forceNewConversation)
            {
                // First time user - start with day_one
                Log("First time user detected, loading day_one message");
                yield return StartCoroutine(LoadPredefinedMessage(aiService,"day_one"));
                lastChatDate = DateTime.Now.Date;
            }
            else if (isNewDay)
            {
                // Existing conversation but new day - load next daily message
                string nextDayIdentifier = GetNextDayIdentifier();
                if (!string.IsNullOrEmpty(nextDayIdentifier))
                {
                    Log($"New day detected, loading: {nextDayIdentifier}");
                    yield return StartCoroutine(LoadPredefinedMessage(aiService, nextDayIdentifier));
                }
                else
                {
                    Log("New day detected but no daily message available, resuming conversation");
                    OnChatInitialized?.Invoke();
                }
                lastChatDate = DateTime.Now.Date;
            }
            else
            {
                Log($"Resuming conversation with {conversationHistory.Count} existing messages");
                // Same day - just resume existing conversation
                OnChatInitialized?.Invoke();
            }

            Log("Chat system initialized successfully with parallel optimization");
        }

        /// <summary>
        /// Handle user message through appropriate flow (predefined or AI)
        /// </summary>
        private IEnumerator HandleUserMessageCoroutine(IChatAIService aiService, IChatRouterService routerService, string userMessage)
        {
            Log($"Processing user message: {userMessage}");
            
            // Add user message to conversation
            conversationHistory.Add(new ChatMessage(Role.user.ToString(), userMessage));
            string conversationContext = BuildConversationContext();

            // Phase 1: Execute initial operations in parallel
            Log("Phase 1: Starting parallel user message processing...");
            RouterResult routerResult = null;
            
            yield return StartCoroutine(ExecuteInParallel(
                SaveUserMessageToConversation(Role.user, userMessage),
                RouteMessageWrapper(routerService, userMessage, conversationContext, (result) => routerResult = result),
                GenerateUpdatedProfile(aiService, userMessage, conversationContext)
            ));
            
            if (routerResult == null || !routerResult.success)
            {
                OnErrorOccurred?.Invoke($"Failed to route message: {routerResult?.errorMessage ?? "Unknown error"}");
                yield break;
            }
            
            Log($"Phase 1 completed: Agent selected = {routerResult.agent}");
            
            // Phase 2: Generate AI response with enhanced instructions (sequential - needs router result)
            Log("Phase 2: Generating AI response with agent-specific instructions...");
            AIResponseResult aiResult = null;

            yield return StartCoroutine(GenerateAIResponseWithInstructions(aiService,
                userMessage,
                routerResult.agent,
                routerResult.examples,
                routerResult.knowledge,
                (result) => aiResult = result));
            
            if (aiResult == null || !aiResult.success)
            {
                OnErrorOccurred?.Invoke($"Failed to generate AI response: {aiResult?.errorMessage ?? "Unknown error"}");
                yield break;
            }
            
            // Add AI response to conversation
            conversationHistory.Add(new ChatMessage(routerResult.agent, aiResult.response));
            
            // Phase 3: Execute AI response operations in parallel
            Log("Phase 3: Starting parallel AI response processing...");
            
            yield return StartCoroutine(ExecuteInParallel(
                SaveBotMessageToConversation(routerResult.agent, aiResult.response),
                UpdateProfileAfterMessage(aiService, aiResult.response)
            ));
            
            // Notify UI
            OnAIMessageReceived?.Invoke(aiResult.response);
            
            Log($"User message processing completed successfully for agent: {routerResult.agent}");
        }

        /// <summary>
        /// Wrapper for router service to work with parallel execution
        /// </summary>
        private IEnumerator RouteMessageWrapper(IChatRouterService routerService, string userMessage, string conversationContext, System.Action<RouterResult> onComplete)
        {
            yield return StartCoroutine(routerService.RouteMessage(userMessage, conversationContext, onComplete));
        }

        /// <summary>
        /// Handle predefined flow button clicks
        /// </summary>
        private IEnumerator HandleButtonClickCoroutine(IChatAIService aiService, string buttonIdentifier)
        {
            Log($"Processing button click: {buttonIdentifier}");
            
            // Add button click as user message
            string userAction = $"[Button: {buttonIdentifier}]";
            conversationHistory.Add(new ChatMessage(Role.user_button.ToString(), userAction));
            
            // Phase 1: Execute user action processing in parallel
            Log("Starting parallel button click processing...");
            
            yield return StartCoroutine(ExecuteInParallel(
                SaveUserMessageToConversation(Role.user_button, userAction),
                UpdateProfileAfterMessage(aiService, userAction)
            ));
            
            // Phase 2: Load next predefined message (sequential - depends on button identifier)
            Log($"Loading predefined message for button: {buttonIdentifier}");
            yield return StartCoroutine(LoadPredefinedMessage(aiService, buttonIdentifier));
            
            Log($"Button click processing completed for: {buttonIdentifier}");
        }

        /// <summary>
        /// Build conversation context string for services
        /// </summary>
        private string BuildConversationContext()
        {
            if (conversationHistory.Count == 0)
                return "";
            
            // Get recent messages (last 10 or so)
            int startIndex = Math.Max(0, conversationHistory.Count - 10);
            var recentMessages = new List<string>();
            
            for (int i = startIndex; i < conversationHistory.Count; i++)
            {
                var msg = conversationHistory[i];
                recentMessages.Add($"{msg.role}: {msg.message}");
            }
            
            return string.Join("\n", recentMessages);
        }

        /// <summary>
        /// Load predefined message from backend by identifier
        /// </summary>
        private IEnumerator LoadPredefinedMessage(IChatAIService aiService, string identifier)
        {
            Log($"Loading predefined message: {identifier}");
            
            bool messageLoaded = false;
            string errorMessage = "";
            string messageText = "";
            string[] buttons = null;

            yield return StartCoroutine(endpoints.CoGetChatPredefinedMessageByIdentifier(identifier, (result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null && dto.data.Count > 0)
                {
                    messageText = dto.data[0].attributes?.content ?? "";
                    buttons = dto.data[0].attributes?.buttons?.ToArray() ?? new string[0];
                    
                    // Add bot message to history
                    conversationHistory.Add(new ChatMessage(Role.pre_defined.ToString(), messageText));
                    
                    // If no buttons, switch to AI flow
                    if (buttons == null || buttons.Length == 0)
                    {
                        Log("No buttons found, switching to AI flow");
                    }
                    
                    messageLoaded = true;
                    Log($"Loaded predefined message: {messageText}");
                }
                else
                {
                    errorMessage = $"Failed to load predefined message '{identifier}': {result}";
                    LogError(errorMessage);
                }
            }));

            yield return new WaitUntil(() => messageLoaded || !string.IsNullOrEmpty(errorMessage));

            if (messageLoaded)
            {
                // Execute predefined message processing in parallel
                Log("Processing predefined message in parallel...");
                
                yield return StartCoroutine(ExecuteInParallel(
                    SaveBotMessageToConversation(Role.pre_defined.ToString(), messageText),
                    UpdateProfileAfterMessage(aiService, messageText)
                ));
                
                // Notify UI
                OnMessageReceived?.Invoke(messageText, buttons);
                OnChatInitialized?.Invoke();
            }
            else
            {
                OnErrorOccurred?.Invoke(errorMessage);
            }
        }

        /// <summary>
        /// Save user message to backend conversation
        /// </summary>
        private IEnumerator SaveUserMessageToConversation(Role userRole, string userMessage)
        {
            if (string.IsNullOrEmpty(currentConversationId))
            {
                Debug.LogWarning("No conversation ID available for saving user message");
                yield break;
            }

            bool messageSaved = false;
            yield return StartCoroutine(endpoints.CoCreateChatConversationMessage(
                userRole.ToString(), 
                userMessage, 
                Guid.Parse(currentConversationId), 
                (result, dto) =>
                {
                    messageSaved = true;
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        Log("User message saved successfully");
                    }
                    else
                    {
                        LogError($"Failed to save user message: {result}");
                    }
                }));

            yield return new WaitUntil(() => messageSaved);
        }

        /// <summary>
        /// Save bot message to backend conversation
        /// </summary>
        private IEnumerator SaveBotMessageToConversation(string agent, string botMessage)
        {
            if (string.IsNullOrEmpty(currentConversationId))
            {
                Debug.LogWarning("No conversation ID available for saving bot message");
                yield break;
            }

            bool messageSaved = false;
            yield return StartCoroutine(endpoints.CoCreateChatConversationMessage(
                agent, 
                botMessage, 
                Guid.Parse(currentConversationId), 
                (result, dto) =>
                {
                    messageSaved = true;
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        Log("Bot message saved successfully");
                    }
                    else
                    {
                        LogError($"Failed to save bot message: {result}");
                    }
                }));

            yield return new WaitUntil(() => messageSaved);
        }

        /// <summary>
        /// Update profile after a message using AI service
        /// </summary>
        private IEnumerator UpdateProfileAfterMessage(IChatAIService aiService, string message)
        {
            if (string.IsNullOrEmpty(currentProfileId))
            {
                Debug.LogWarning("No profile ID available for updating profile");
                yield break;
            }

            Log($"Updating profile after message: {message}");
            
            // Build conversation context for AI profile generation
            string conversationContext = BuildConversationContext();
            
            // Generate updated profile using AI service
            yield return StartCoroutine(GenerateUpdatedProfile(aiService, message, conversationContext));
        }

        /// <summary>
        /// Create or get existing conversation
        /// </summary>
        private IEnumerator CreateOrGetConversation(bool forceNew = false)
        {
            if (!forceNew && !string.IsNullOrEmpty(currentConversationId))
            {
                // Check if conversation still exists
                bool conversationExists = false;
                yield return StartCoroutine(endpoints.CoGetChatConversation(Guid.Parse(currentConversationId), (result, dto) =>
                {
                    conversationExists = result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null;
                }));
                
                if (conversationExists)
                {
                    Log($"Using existing conversation: {currentConversationId}");
                    yield break;
                }
            }

            // Create new conversation
            bool conversationCreated = false;
            yield return StartCoroutine(endpoints.CoCreateChatConversation((result, dto) =>
            {
                conversationCreated = true;
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    currentConversationId = dto.data.id;
                    Log($"Created new conversation: {currentConversationId}");
                }
                else
                {
                    LogError($"Failed to create conversation: {result}");
                }
            }));

            yield return new WaitUntil(() => conversationCreated);
        }

        /// <summary>
        /// Create or get existing chat profile
        /// </summary>
        private IEnumerator CreateOrGetChatProfile(bool forceNew = false)
        {
            if (!forceNew && !string.IsNullOrEmpty(currentConversationId))
            {
                // Try to get existing profile using conversation ID
                bool profileChecked = false;
                bool profileExists = false;
                
                yield return StartCoroutine(endpoints.CoGetChatProfile(Guid.Parse(currentConversationId), (result, dto) =>
                {
                    profileChecked = true;
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                    {
                        currentProfile = dto.data.attributes.profile;
                        currentProfileId = dto.data.id;
                        profileExists = true;
                        Log($"Found existing profile: {currentProfileId}");
                    }
                    else
                    {
                        Log($"No existing profile found for conversation {currentConversationId}");
                    }
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
                profileCreated = true;
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
            }));

            yield return new WaitUntil(() => profileCreated);
        }

        /// <summary>
        /// Load conversation history from backend
        /// </summary>
        private IEnumerator LoadConversationHistory()
        {
            if (string.IsNullOrEmpty(currentConversationId))
            {
                Debug.LogWarning("No conversation ID available for loading history");
                yield break;
            }

            bool historyLoaded = false;
            yield return StartCoroutine(endpoints.CoGetChatConversationMessages((result, dto) =>
            {
                historyLoaded = true;
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    conversationHistory.Clear();
                    foreach (var message in dto.data)
                    {
                        conversationHistory.Add(new ChatMessage(
                            message.attributes.role, 
                            message.attributes.message
                        ));
                    }
                    Log($"Loaded {conversationHistory.Count} messages from conversation history");
                }
                else
                {
                    LogError($"Failed to load conversation history: {result}");
                }
            }, Guid.Parse(currentConversationId)));

            yield return new WaitUntil(() => historyLoaded);
        }

        /// <summary>
        /// Detect if this is a new day since last chat
        /// </summary>
        private bool DetectNewDay()
        {
            return lastChatDate.Date < DateTime.Now.Date;
        }

        /// <summary>
        /// Get the next day identifier based on conversation history
        /// </summary>
        private string GetNextDayIdentifier()
        {
            // Count how many daily messages have been sent
            int dayCount = conversationHistory.Count(m => m.role == Role.pre_defined.ToString() && 
                                                    (m.message?.Contains("day_") == true || 
                                                     m.message?.Contains("daily") == true));
            
            // Return next day identifier (day_two, day_three, etc.)
            string nextDay = $"day_{GetDayName(dayCount + 2)}"; // +2 because day_one is first
            return nextDay;
        }

        /// <summary>
        /// Convert day number to word
        /// </summary>
        private string GetDayName(int dayNumber)
        {
            return dayNumber switch
            {
                1 => "one",
                2 => "two", 
                3 => "three",
                4 => "four",
                5 => "five",
                6 => "six",
                7 => "seven",
                8 => "eight",
                9 => "nine",
                10 => "ten",
                _ => dayNumber.ToString() // fallback to number
            };
        }

        /// <summary>
        /// Generate updated profile using AI service with cached instructions and conversation history
        /// </summary>
        private IEnumerator GenerateUpdatedProfile(IChatAIService aiService, string newMessage, string conversationContext)
        {
            Log($"Generating updated profile using dedicated profile method for message: {newMessage}");
            
            // Get cached profile generation instructions
            string profileInstructions = GetProfileGeneratorInstruction();
            
            if (string.IsNullOrEmpty(profileInstructions))
            {
                Debug.LogWarning("No profile generator instructions available, skipping profile update");
                yield break;
            }

            // Use dedicated profile generation method
            ProfileGenerationResult profileResult = null;
            
            yield return StartCoroutine(aiService.GenerateProfile(
                newMessage,
                currentProfile,
                conversationContext,
                profileInstructions,
                (result) => profileResult = result));

            if (profileResult != null && profileResult.success)
            {
                // Update current profile with AI-generated result
                string newProfile = profileResult.updatedProfile;
                currentProfile = newProfile;
                
                // Save updated profile to backend
                yield return StartCoroutine(SaveProfileToBackend(newProfile));
                
                Log($"Profile updated successfully using dedicated profile method: {newProfile.Substring(0, Math.Min(100, newProfile.Length))}...");
            }
            else
            {
                Debug.LogWarning($"Failed to generate profile update: {profileResult?.errorMessage ?? "Unknown error"}");
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
                Debug.LogWarning("No profile ID available for saving profile");
                yield break;
            }

            bool profileSaved = false;
            yield return StartCoroutine(endpoints.CoUpdateChatProfile(Guid.Parse(currentProfileId), profileContent, (result, dto) =>
            {
                profileSaved = true;
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Log("Profile saved to backend successfully");
                }
                else
                {
                    LogError($"Failed to save profile to backend: {result}");
                }
            }));

            yield return new WaitUntil(() => profileSaved);
        }

        #endregion

        private void LogError(string message)
        {
            if (isLogging)
            {
                Debug.LogError(message);
            }
        }

        private void Log(string message)
        {
            if (isLogging)
            {
                Debug.Log(message);
            }
        }

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
                loadCompleted = true;
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto?.data != null)
                {
                    instructionsCache.Clear();
                    
                    foreach (var instruction in dto.data)
                    {
                        if (!string.IsNullOrEmpty(instruction.attributes?.identifier) && 
                            !string.IsNullOrEmpty(instruction.attributes?.instruction))
                        {
                            instructionsCache[instruction.attributes.identifier] = instruction.attributes.instruction;
                            Log($"Cached instruction for: {instruction.attributes.identifier}");
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
            if (instructionsCache.TryGetValue(identifier, out string instruction))
            {
                return instruction;
            }
            
            Debug.LogWarning($"No instruction found for identifier: {identifier}");
            return "";
        }

        /// <summary>
        /// Get agent-specific instruction for AI generation
        /// </summary>
        /// <param name="agentName">The name of the agent</param>
        /// <returns>Agent instruction or empty string if not found</returns>
        private string GetAgentInstruction(string agentName)
        {
            return GetInstruction(agentName);
        }

        /// <summary>
        /// Get profile generator instruction
        /// </summary>
        /// <returns>Profile generator instruction or empty string if not found</returns>
        private string GetProfileGeneratorInstruction()
        {
            return GetInstruction("profile_generator");
        }

        /// <summary>
        /// Check if instructions are loaded and available
        /// </summary>
        /// <returns>True if instructions are loaded, false otherwise</returns>
        private bool AreInstructionsAvailable()
        {
            return instructionsLoaded && instructionsCache.Count > 0;
        }

        /// <summary>
        /// Enhanced AI response generation with agent-specific instructions and streaming support
        /// </summary>
        private IEnumerator GenerateAIResponseWithInstructions(IChatAIService aiService, string userMessage, string agent, string examples, string knowledge, System.Action<AIResponseResult> onComplete)
        {
            // Get agent-specific instruction
            string agentInstruction = GetAgentInstruction(agent);
            
            if (string.IsNullOrEmpty(agentInstruction))
            {
                Debug.LogWarning($"No specific instruction found for agent: {agent}, using fallback instruction");
                agentInstruction = $"You are {agent}. Respond helpfully to the user's message."; // Basic fallback
            }
            
            Log($"Generating AI response for agent '{agent}' with instruction length: {agentInstruction.Length}");
            
            // Build conversation history for context
            string conversationContext = BuildConversationContext();
            
            yield return StartCoroutine(aiService.GenerateResponse(
                userMessage,
                agentInstruction,
                examples,
                knowledge,
                currentProfile,
                conversationContext, // Add conversation history
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
                Debug.LogWarning("ExecuteInParallel called with no coroutines");
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
        /// Execute multiple coroutines in parallel with individual result tracking
        /// </summary>
        /// <typeparam name="T">Type of results to collect</typeparam>
        /// <param name="tasks">Array of parallel task definitions</param>
        /// <returns>Array of results in the same order as input tasks</returns>
        private IEnumerator ExecuteInParallelWithResults<T>(ParallelTask<T>[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
            {
                Debug.LogWarning("ExecuteInParallelWithResults called with no tasks");
                yield break;
            }

            // Start all tasks
            bool[] completionFlags = new bool[tasks.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                int index = i; // Capture for closure
                StartCoroutine(ExecuteParallelTask(tasks[i], index, completionFlags));
            }

            // Wait for all to complete
            yield return new WaitUntil(() => AllCoroutinesComplete(completionFlags));

            Log($"ExecuteInParallelWithResults: All {tasks.Length} tasks completed");
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
        /// Helper to execute a parallel task and capture its result
        /// </summary>
        private IEnumerator ExecuteParallelTask<T>(ParallelTask<T> task, int index, bool[] completionFlags)
        {
            yield return StartCoroutine(task.coroutine);
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

        /// <summary>
        /// Convenience method for executing 4 coroutines in parallel
        /// </summary>
        private IEnumerator ExecuteInParallel(IEnumerator coroutine1, IEnumerator coroutine2, IEnumerator coroutine3, IEnumerator coroutine4)
        {
            yield return StartCoroutine(ExecuteInParallel(new IEnumerator[] { coroutine1, coroutine2, coroutine3, coroutine4 }));
        }

        #endregion
    }
}