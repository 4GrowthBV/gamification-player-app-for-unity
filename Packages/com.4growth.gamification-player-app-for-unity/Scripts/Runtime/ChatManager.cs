using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using GamificationPlayer.DTO.Chat;
using GamificationPlayer.Session;

namespace GamificationPlayer
{
    public class ChatManager : MonoBehaviour
    {
        #region Dependencies
        private GamificationPlayerEndpoints endpoints;
        private SessionLogData sessionData;
        private bool isInitialized = false;
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initialize ChatManager with required dependencies
        /// </summary>
        /// <param name="gamificationPlayerEndpoints">The endpoints instance for API calls</param>
        /// <param name="sessionLogData">The session data instance for data access</param>
        public void Initialize(GamificationPlayerEndpoints gamificationPlayerEndpoints, SessionLogData sessionLogData)
        {
            endpoints = gamificationPlayerEndpoints;
            sessionData = sessionLogData;
            isInitialized = true;
        }

        /// <summary>
        /// Check if ChatManager has been properly initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        public bool IsInitialized()
        {
            return isInitialized && endpoints != null && sessionData != null;
        }
        
        #endregion

        #region Chat State
        [SerializeField] private string currentConversationId;
        [SerializeField] private string currentProfileId;
        [SerializeField] private bool isInPredefinedFlow = true;
        [SerializeField] private List<ChatMessage> conversationHistory = new List<ChatMessage>();
        
        [Serializable]
        public class ChatMessage
        {
            public string role; // "user" or "bot"
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

        #region Events
        public static event Action<string, string[]> OnMessageReceived; // message, buttons
        public static event Action<string> OnAIMessageReceived; // AI response
        public static event Action<string> OnErrorOccurred;
        public static event Action OnChatInitialized;
        #endregion

        #region Public API
        
        /// <summary>
        /// Initialize the chat system - reuses existing conversation/profile or creates new ones
        /// </summary>
        public void InitializeChat()
        {
            InitializeChat(false);
        }

        /// <summary>
        /// Initialize the chat system with option to force new conversation
        /// </summary>
        /// <param name="forceNewConversation">If true, creates new conversation even if existing one found</param>
        public void InitializeChat(bool forceNewConversation)
        {
            if (!IsInitialized())
            {
                Debug.LogWarning("ChatManager is not initialized. Please call Initialize() first.");
                OnErrorOccurred?.Invoke("ChatManager not properly initialized");
                return;
            }

            // Check if the GameObject is still valid before starting coroutine
            if (this == null || gameObject == null)
            {
                Debug.LogError("ChatManager GameObject has been destroyed");
                return;
            }
            
            StartCoroutine(InitializeChatCoroutine(forceNewConversation));
        }

        /// <summary>
        /// Handle user clicking a button from a predefined message
        /// </summary>
        /// <param name="buttonIdentifier">The identifier of the button clicked</param>
        public void HandleButtonClick(string buttonIdentifier)
        {
            if (string.IsNullOrEmpty(buttonIdentifier))
            {
                Debug.LogError("Button identifier is null or empty");
                return;
            }

            if (!IsInitialized())
            {
                Debug.LogWarning("ChatManager is not initialized. Please call Initialize() first.");
                return;
            }

            // Check if the GameObject is still valid before starting coroutine
            if (this == null || gameObject == null)
            {
                Debug.LogError("ChatManager GameObject has been destroyed");
                return;
            }

            StartCoroutine(HandleButtonClickCoroutine(buttonIdentifier));
        }

        /// <summary>
        /// Handle user typing a free text message (AI flow)
        /// </summary>
        /// <param name="userMessage">The user's message</param>
        public void HandleUserMessage(string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage))
            {
                Debug.LogError("User message is null or empty");
                return;
            }

            if (!IsInitialized())
            {
                Debug.LogWarning("ChatManager is not initialized. Please call Initialize() first.");
                return;
            }

            // Check if the GameObject is still valid before proceeding
            if (this == null || gameObject == null)
            {
                Debug.LogError("ChatManager GameObject has been destroyed");
                return;
            }

            // Add user message to local history
            AddMessageToHistory("user", userMessage);
            
            StartCoroutine(HandleUserMessageCoroutine(userMessage));
        }

        /// <summary>
        /// Get the current conversation history
        /// </summary>
        /// <returns>List of chat messages</returns>
        public List<ChatMessage> GetConversationHistory()
        {
            return new List<ChatMessage>(conversationHistory);
        }

        /// <summary>
        /// Check if currently in predefined message flow
        /// </summary>
        /// <returns>True if in predefined flow, false if in AI flow</returns>
        public bool IsInPredefinedFlow()
        {
            return isInPredefinedFlow;
        }
        #endregion

        #region Private Implementation

        private IEnumerator InitializeChatCoroutine(bool forceNewConversation = false)
        {
            Debug.Log($"Initializing Wellbe Buddy Chat... (Force new: {forceNewConversation})");

            // Step 1: Create or load conversation FIRST (required for profile creation)
            yield return StartCoroutine(CreateOrGetConversation(forceNewConversation));
            
            if (string.IsNullOrEmpty(currentConversationId))
            {
                OnErrorOccurred?.Invoke("Failed to create or get conversation");
                yield break;
            }

            // Step 2: Create or check chat profile (using conversation ID)
            yield return StartCoroutine(CreateOrGetChatProfile(forceNewConversation));
            
            if (string.IsNullOrEmpty(currentProfileId))
            {
                OnErrorOccurred?.Invoke("Failed to create or get chat profile");
                yield break;
            }

            // Step 3: Load conversation history if it exists
            yield return StartCoroutine(LoadConversationHistory());

            // Step 4: Start with first predefined message (only if no history or forced new conversation)
            if (conversationHistory.Count == 0 || forceNewConversation)
            {
                yield return StartCoroutine(LoadPredefinedMessage("day_one"));
            }
            else
            {
                Debug.Log($"Resuming conversation with {conversationHistory.Count} existing messages");
                // Trigger event to show we're ready but don't load day_one
                OnChatInitialized?.Invoke();
            }

            Debug.Log("Wellbe Buddy Chat initialized successfully");
        }

        private IEnumerator CreateOrGetChatProfile(bool forceNew = false)
        {
            Debug.Log($"Creating or getting chat profile... (Force new: {forceNew})");
            
            // Check if we already have a profile ID from existing conversation
            if (!forceNew && !string.IsNullOrEmpty(currentProfileId))
            {
                Debug.Log($"✅ Using existing chat profile from conversation: {currentProfileId}");
                yield break;
            }
            
            // Try to get existing profile from session data (unless forcing new)
            if (!forceNew && sessionData.TryGetLatestChatProfileId(out Guid existingProfileId))
            {
                currentProfileId = existingProfileId.ToString();
                Debug.Log($"✅ Using existing chat profile from session: {currentProfileId}");
                yield break;
            }

            // Create new profile using the conversation ID we created earlier
            if (string.IsNullOrEmpty(currentConversationId))
            {
                string error = "Cannot create profile without conversation ID";
                Debug.LogError($"❌ {error}");
                OnErrorOccurred?.Invoke(error);
                yield break;
            }

            bool profileCreated = false;
            string errorMessage = "";

            Debug.Log($"Creating new chat profile for conversation: {currentConversationId}");
            StartCoroutine(endpoints.CoCreateChatProfile("wellbe_buddy", Guid.Parse(currentConversationId), (result, dto) =>
            {
                if (result == UnityWebRequest.Result.Success && dto?.data != null)
                {
                    currentProfileId = dto.data.id;
                    profileCreated = true;
                    Debug.Log($"✅ Successfully created chat profile: {currentProfileId} for conversation: {currentConversationId}");
                }
                else
                {
                    errorMessage = $"Failed to create chat profile: {result}";
                    Debug.LogError($"❌ {errorMessage}");
                }
            }));

            // Wait for completion
            yield return new WaitUntil(() => profileCreated || !string.IsNullOrEmpty(errorMessage));

            if (!profileCreated)
            {
                OnErrorOccurred?.Invoke(errorMessage);
            }
        }

        private IEnumerator CreateOrGetConversation(bool forceNew = false)
        {
            Debug.Log($"Creating or getting conversation... (Force new: {forceNew})");
            
            // Check API for existing conversations (unless forcing new)
            if (!forceNew)
            {
                bool conversationCheckComplete = false;
                string existingConversationId = null;
                string existingProfileId = null;
                
                Debug.Log("Checking API for existing conversations...");
                StartCoroutine(endpoints.CoGetChatConversations((result, dto) =>
                {
                    if (result == UnityWebRequest.Result.Success && dto?.data != null && dto.data.Count > 0)
                    {
                        // Found existing conversation(s) - use the most recent one
                        var conversation = dto.data[0];
                        existingConversationId = conversation.id;
                        
                        // Try to extract the profile ID from the conversation relationships
                        // The API response includes profile data in the relationships section
                        if (dto.included != null)
                        {
                            foreach (var item in dto.included)
                            {
                                if (item.type == "chat_profile")
                                {
                                    existingProfileId = item.id;
                                    break; // Use the first profile found
                                }
                            }
                        }
                        
                        Debug.Log($"✅ Found existing conversation via API: {existingConversationId}");
                        if (!string.IsNullOrEmpty(existingProfileId))
                        {
                            Debug.Log($"✅ Found existing profile via API: {existingProfileId}");
                        }
                    }
                    else
                    {
                        Debug.Log("No existing conversations found via API");
                    }
                    conversationCheckComplete = true;
                }));
                
                yield return new WaitUntil(() => conversationCheckComplete);
                
                // If we found an existing conversation, use it
                if (!string.IsNullOrEmpty(existingConversationId))
                {
                    currentConversationId = existingConversationId;
                    Debug.Log($"✅ Using existing conversation from API: {currentConversationId}");
                    
                    // Also set the profile ID if we found one
                    if (!string.IsNullOrEmpty(existingProfileId))
                    {
                        currentProfileId = existingProfileId;
                        Debug.Log($"✅ Using existing profile from API: {currentProfileId}");
                    }
                    
                    yield break;
                }
            }
            else
            {
                Debug.Log("Skipping API check - forcing new conversation");
            }

            // Create new conversation
            bool conversationCreated = false;
            string errorMessage = "";

            Debug.Log("Creating new chat conversation via API...");
            StartCoroutine(endpoints.CoCreateChatConversation((result, dto) =>
            {
                if (result == UnityWebRequest.Result.Success && dto?.data != null)
                {
                    currentConversationId = dto.data.id;
                    conversationCreated = true;
                    Debug.Log($"✅ Successfully created new conversation via API: {currentConversationId}");
                }
                else
                {
                    errorMessage = $"Failed to create conversation: {result}";
                    Debug.LogError($"❌ {errorMessage}");
                }
            }));

            // Wait for completion
            yield return new WaitUntil(() => conversationCreated || !string.IsNullOrEmpty(errorMessage));

            if (!conversationCreated)
            {
                OnErrorOccurred?.Invoke(errorMessage);
            }
        }

        private IEnumerator LoadConversationHistory()
        {
            Debug.Log("Loading conversation history...");
            
            if (string.IsNullOrEmpty(currentConversationId))
            {
                Debug.Log("No conversation ID, skipping history loading");
                yield break;
            }

            conversationHistory.Clear();
            bool historyLoaded = false;

            // Load messages from the API
            StartCoroutine(endpoints.CoGetChatConversationMessages((result, dto) =>
            {
                if (result == UnityWebRequest.Result.Success && dto?.data != null)
                {
                    Debug.Log($"Retrieved {dto.data.Count} messages from API for conversation {currentConversationId}");
                    
                    // NOTE: The API should filter by conversation ID, but if it returns messages from other conversations,
                    // we'll still process them for now. This might be an API server issue.
                    
                    // Convert API messages to local ChatMessage format
                    foreach (var messageData in dto.data)
                    {
                        if (messageData.attributes != null)
                        {
                            var chatMessage = new ChatMessage(
                                messageData.attributes.role,
                                messageData.attributes.message
                            );
                            // Use the actual timestamp from the API
                            chatMessage.timestamp = messageData.attributes.CreatedAt;
                            conversationHistory.Add(chatMessage);
                        }
                    }
                    
                    // Sort by timestamp to ensure correct order
                    conversationHistory.Sort((a, b) => DateTime.Compare(a.timestamp, b.timestamp));
                    
                    Debug.Log($"✅ Loaded {conversationHistory.Count} messages from conversation history");
                    historyLoaded = true;
                }
                else
                {
                    // Don't treat this as an error - conversation might be new with no messages
                    Debug.Log($"No messages found for conversation {currentConversationId} or failed to load: {result}");
                    historyLoaded = true;
                }
            }, Guid.Parse(currentConversationId), 1, 50)); // Load first 50 messages

            yield return new WaitUntil(() => historyLoaded);
            
            Debug.Log($"Conversation history loading complete: {conversationHistory.Count} messages");
        }

        private IEnumerator LoadPredefinedMessage(string identifier)
        {
            Debug.Log($"Loading predefined message: {identifier}");
            
            bool messageLoaded = false;
            string errorMessage = "";

            StartCoroutine(endpoints.CoGetChatPredefinedMessageByIdentifier(identifier, (result, dto) =>
            {
                if (result == UnityWebRequest.Result.Success && dto?.data != null)
                {
                    string messageText = dto.data[0].attributes?.content ?? "";
                    string[] buttons = dto.data[0].attributes?.buttons?.ToArray() ?? new string[0];
                    
                    // Add bot message to history
                    AddMessageToHistory("bot", messageText);
                    
                    // If no buttons, switch to AI flow
                    if (buttons == null || buttons.Length == 0)
                    {
                        isInPredefinedFlow = false;
                        Debug.Log("No buttons found, switching to AI flow");
                    }
                    
                    OnMessageReceived?.Invoke(messageText, buttons);
                    messageLoaded = true;
                    
                    Debug.Log($"Loaded predefined message: {messageText}");
                }
                else
                {
                    errorMessage = $"Failed to load predefined message '{identifier}': {result}";
                    Debug.LogError(errorMessage);
                }
            }));

            yield return new WaitUntil(() => messageLoaded || !string.IsNullOrEmpty(errorMessage));

            if (!messageLoaded)
            {
                OnErrorOccurred?.Invoke(errorMessage);
            }
        }

        private IEnumerator HandleButtonClickCoroutine(string buttonIdentifier)
        {
            Debug.Log($"Handling button click: {buttonIdentifier}");
            
            // Add user action to history (button click)
            AddMessageToHistory("user", $"[Button: {buttonIdentifier}]");
            
            // Load next predefined message
            yield return StartCoroutine(LoadPredefinedMessage(buttonIdentifier));
        }

        private IEnumerator HandleUserMessageCoroutine(string userMessage)
        {
            Debug.Log($"Handling user message: {userMessage}");
            
            // Switch to AI flow
            isInPredefinedFlow = false;
            
            // Step 1: Get conversation history string
            string historyString = BuildHistoryString();
            
            // Step 2: Send to Router service
            RouterResponse routerResponse = null;
            yield return StartCoroutine(CallRouterService(userMessage, historyString, (response) => routerResponse = response));
            
            if (routerResponse == null || string.IsNullOrEmpty(routerResponse.agent))
            {
                OnErrorOccurred?.Invoke("Failed to get agent from Router service");
                yield break;
            }
            
            // Step 3: Send to Memory service
            string updatedProfile = "";
            yield return StartCoroutine(CallMemoryService(userMessage, historyString, (profile) => updatedProfile = profile));
            
            // Step 4: Get agent instructions
            string instructions = "";
            yield return StartCoroutine(GetAgentInstructions(routerResponse.agent, (inst) => instructions = inst));
            
            // Step 5: Generate AI response (this would typically call an AI service)
            string aiResponse = GenerateAIResponse(userMessage, historyString, routerResponse.agent, routerResponse.examples, routerResponse.knowledge, updatedProfile, instructions);
            
            // Step 6: Add AI response to history and notify
            AddMessageToHistory("bot", aiResponse);
            OnAIMessageReceived?.Invoke(aiResponse);
            
            // Step 7: Save messages to conversation
            yield return StartCoroutine(SaveMessagesToConversation(userMessage, aiResponse));
        }

        private string BuildHistoryString()
        {
            if (conversationHistory.Count == 0)
                return "";

            // Get last few messages (limit to avoid too long strings)
            var recentMessages = conversationHistory.TakeLast(6).ToList();
            
            var historyParts = recentMessages.Select(msg => $"{msg.role}: {msg.message}");
            return string.Join(" / ", historyParts);
        }

        [Serializable]
        public class RouterResponse
        {
            public string agent = "";
            public string examples = "";
            public string knowledge = "";
        }

        private IEnumerator CallRouterService(string userMessage, string history, System.Action<RouterResponse> onComplete)
        {
            Debug.Log("Calling Router service...");
            
            var response = new RouterResponse();
            
            // This would call the actual Router API endpoint
            // For now, we'll simulate the response
            yield return new WaitForSeconds(0.5f); // Simulate network delay
            
            // Simulate router decision based on message content
            if (userMessage.ToLower().Contains("stress") || userMessage.ToLower().Contains("anxiety"))
            {
                response.agent = "mindfulness";
                response.examples = "Take deep breaths, try meditation";
                response.knowledge = "Stress management techniques";
            }
            else if (userMessage.ToLower().Contains("legal") || userMessage.ToLower().Contains("rights"))
            {
                response.agent = "legal";
                response.examples = "Know your rights, seek legal advice";
                response.knowledge = "Legal information database";
            }
            else if (userMessage.ToLower().Contains("work") || userMessage.ToLower().Contains("hr"))
            {
                response.agent = "hr";
                response.examples = "Workplace policies, HR support";
                response.knowledge = "HR knowledge base";
            }
            else
            {
                response.agent = "general";
                response.examples = "General wellness advice";
                response.knowledge = "General wellbeing information";
            }
            
            Debug.Log($"Router selected agent: {response.agent}");
            onComplete?.Invoke(response);
        }

        private IEnumerator CallMemoryService(string userMessage, string history, System.Action<string> onComplete)
        {
            Debug.Log("Calling Memory service...");
            
            // This would call the actual Memory API endpoint
            // For now, we'll simulate the response
            yield return new WaitForSeconds(0.3f); // Simulate network delay
            
            // Simulate profile update
            string profile = "Updated user profile with recent conversation context";
            
            Debug.Log("Memory service updated profile");
            onComplete?.Invoke(profile);
        }

        private IEnumerator GetAgentInstructions(string agent, System.Action<string> onComplete)
        {
            Debug.Log($"Getting instructions for agent: {agent}");
            
            bool instructionsLoaded = false;
            string errorMessage = "";
            string instructions = "";

            StartCoroutine(endpoints.CoGetChatInstructionByAgent(agent, (result, dto) =>
            {
                if (result == UnityWebRequest.Result.Success && dto?.data != null)
                {
                    instructions = dto.data[0].attributes?.instruction ?? "";
                    instructionsLoaded = true;
                    Debug.Log($"Loaded instructions: {instructions}");
                }
                else
                {
                    errorMessage = $"Failed to load instructions for agent '{agent}': {result}";
                    Debug.LogError(errorMessage);
                    // Use default instructions
                    instructions = "Be helpful and supportive in your responses.";
                    instructionsLoaded = true;
                }
            }));

            yield return new WaitUntil(() => instructionsLoaded || !string.IsNullOrEmpty(errorMessage));
            onComplete?.Invoke(instructions);
        }

        private string GenerateAIResponse(string userMessage, string history, string agent, string examples, string knowledge, string profile, string instructions)
        {
            Debug.Log("Generating AI response...");
            
            // This is where you would typically call an AI service (OpenAI, etc.)
            // For now, we'll create a simple response based on the agent
            
            string response = agent switch
            {
                "mindfulness" => $"I understand you're dealing with stress. {instructions} Try taking a few deep breaths. {examples}",
                "legal" => $"For legal matters, {instructions} {examples} Please consult with a qualified legal professional.",
                "hr" => $"Regarding workplace concerns, {instructions} {examples} Consider speaking with your HR department.",
                _ => $"Thank you for sharing. {instructions} I'm here to support your wellbeing journey."
            };
            
            Debug.Log($"Generated AI response: {response}");
            return response;
        }

        private IEnumerator SaveMessagesToConversation(string userMessage, string aiResponse)
        {
            Debug.Log("Saving messages to conversation...");
            
            if (string.IsNullOrEmpty(currentConversationId))
            {
                Debug.LogWarning("No conversation ID available for saving messages");
                yield break;
            }

            // Save user message
            bool userMessageSaved = false;
            StartCoroutine(endpoints.CoCreateChatConversationMessage(
                "user", 
                userMessage, 
                Guid.Parse(currentConversationId), 
                (result, dto) =>
                {
                    userMessageSaved = true;
                    if (result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("User message saved successfully");
                    }
                    else
                    {
                        Debug.LogError($"Failed to save user message: {result}");
                    }
                }));

            yield return new WaitUntil(() => userMessageSaved);

            // Save AI response
            bool aiMessageSaved = false;
            StartCoroutine(endpoints.CoCreateChatConversationMessage(
                "bot", 
                aiResponse, 
                Guid.Parse(currentConversationId), 
                (result, dto) =>
                {
                    aiMessageSaved = true;
                    if (result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("AI message saved successfully");
                    }
                    else
                    {
                        Debug.LogError($"Failed to save AI message: {result}");
                    }
                }));

            yield return new WaitUntil(() => aiMessageSaved);
        }

        private void AddMessageToHistory(string role, string message)
        {
            conversationHistory.Add(new ChatMessage(role, message));
            
            // Limit history size to prevent memory issues
            if (conversationHistory.Count > 50)
            {
                conversationHistory.RemoveAt(0);
            }
            
            Debug.Log($"Added to history - {role}: {message}");
        }
        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Clear all chat data and reset to initial state
        /// </summary>
        public void ResetChat()
        {
            currentConversationId = "";
            currentProfileId = "";
            isInPredefinedFlow = true;
            conversationHistory.Clear();
            
            Debug.Log("Chat reset to initial state");
        }

        /// <summary>
        /// Get current chat state information
        /// </summary>
        /// <returns>Chat state info</returns>
        public string GetChatStateInfo()
        {
            return $"Profile: {currentProfileId}, Conversation: {currentConversationId}, " +
                   $"Messages: {conversationHistory.Count}, Predefined Flow: {isInPredefinedFlow}";
        }
        #endregion
    }
}