using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamificationPlayer.DTO.Chat;
using GamificationPlayer.Session;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// API wrapper for chat database operations using actual GamificationPlayerEndpoints
    /// Integrates with real API endpoints and provides local caching
    /// </summary>
    public class ChatDatabaseAPI
    {
        #region Private Fields
        
        private ChatDataCache dataCache;
        private GamificationPlayerEndpoints endpoints;
        private EditorCoroutineRunner coroutineRunner;
        private SessionLogData sessionData;
        private EnvironmentConfig environmentConfig;
        
        // Editor context IDs (required when sessionData is empty)
        private string organizationId;
        private string microGameId;
        
        #endregion
        
        #region Constructor
        
        public ChatDatabaseAPI(ChatDataCache cache, EnvironmentConfig envConfig)
        {
            dataCache = cache;
            environmentConfig = envConfig;
            
            if (environmentConfig != null)
            {
                sessionData = new SessionLogData();
                endpoints = new GamificationPlayerEndpoints(environmentConfig, sessionData);
            }
            
            InitializeCoroutineRunner();
        }
        
        private void InitializeCoroutineRunner()
        {
            coroutineRunner = UnityEngine.Object.FindFirstObjectByType<EditorCoroutineRunner>();
            if (coroutineRunner == null)
            {
                var go = new GameObject("Chat Database API Coroutine Runner");
                coroutineRunner = go.AddComponent<EditorCoroutineRunner>();
            }
        }
        
        /// <summary>
        /// Set organization and microgame IDs for Editor context API calls
        /// Required because sessionData is empty in Editor context
        /// </summary>
        public void SetContextIds(string orgId, string microGameId)
        {
            this.organizationId = orgId;
            this.microGameId = microGameId;
        }
        
        /// <summary>
        /// Check if context IDs are properly set for API calls
        /// </summary>
        public bool HasValidContextIds()
        {
            return !string.IsNullOrEmpty(organizationId) && !string.IsNullOrEmpty(microGameId);
        }
        
        #endregion
        
        #region Chat Profiles
        
        /// <summary>
        /// Get all chat profiles - Note: API doesn't have GET endpoint for profiles, returns cached data
        /// </summary>
        public void GetAllProfiles(Action<List<ChatProfileData>, string> onComplete)
        {
            try
            {
                var profiles = dataCache.GetAllProfiles();
                onComplete?.Invoke(profiles, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(null, ex.Message);
            }
        }
        
        /// <summary>
        /// Create a new chat profile using the real API endpoint
        /// </summary>
        public void CreateProfile(ChatProfileData profile, Action<bool, string> onComplete)
        {
            if (profile == null)
            {
                onComplete?.Invoke(false, "Profile data is null");
                return;
            }
            
            if (endpoints == null)
            {
                // Fallback to local creation if no endpoints available
                profile.status = "new";
                dataCache.AddProfile(profile);
                Debug.Log($"Profile '{profile.name}' created locally (no API connection)");
                onComplete?.Invoke(true, null);
                return;
            }
            
            // Use real API endpoint
            coroutineRunner.StartCoroutine(CreateProfileCoroutine(profile, onComplete));
        }
        
        private IEnumerator CreateProfileCoroutine(ChatProfileData profile, Action<bool, string> onComplete)
        {
            yield return endpoints.CoCreateChatProfile(profile.name, System.Guid.Parse(profile.userId), (result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null)
                {
                    // Update profile with server data
                    profile.id = dto.data.id;
                    profile.createdAt = dto.data.attributes.CreatedAt;
                    profile.lastModified = dto.data.attributes.UpdatedAt;
                    profile.status = "synced";
                    
                    dataCache.AddProfile(profile);
                    Debug.Log($"Profile '{profile.name}' created successfully via API");
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    profile.status = "error";
                    Debug.LogError($"Failed to create profile via API: {result}");
                    onComplete?.Invoke(false, $"API Error: {result}");
                }
            });
        }
        
        /// <summary>
        /// Update an existing chat profile locally
        /// </summary>
        public void UpdateProfile(ChatProfileData profile, Action<bool, string> onComplete)
        {
            try
            {
                if (profile == null)
                {
                    onComplete?.Invoke(false, "Profile data is null");
                    return;
                }
                
                dataCache.UpdateProfile(profile);
                
                Debug.Log($"Profile '{profile.name}' updated locally");
                onComplete?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        /// <summary>
        /// Delete a chat profile locally
        /// </summary>
        public void DeleteProfile(string profileId, Action<bool, string> onComplete)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                {
                    onComplete?.Invoke(false, "Profile ID is null or empty");
                    return;
                }
                
                var profile = dataCache.GetProfile(profileId);
                if (profile == null)
                {
                    onComplete?.Invoke(false, "Profile not found");
                    return;
                }
                
                dataCache.RemoveProfile(profileId);
                
                Debug.Log($"Profile '{profile.name}' deleted locally");
                onComplete?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        #endregion
        
        #region Chat Conversations
        
        /// <summary>
        /// Get conversations using the real API endpoint
        /// </summary>
        public void GetAllConversations(Action<List<ChatConversationData>, string> onComplete)
        {
            if (endpoints == null)
            {
                // Fallback to cached data
                try
                {
                    var conversations = new List<ChatConversationData>();
                    // Get all conversations from cache (since we don't have profile filtering in cache method)
                    foreach (var profile in dataCache.GetAllProfiles())
                    {
                        conversations.AddRange(dataCache.GetConversationsForProfile(profile.id));
                    }
                    onComplete?.Invoke(conversations, null);
                }
                catch (Exception ex)
                {
                    onComplete?.Invoke(null, ex.Message);
                }
                return;
            }
            
            coroutineRunner.StartCoroutine(GetConversationsCoroutine(onComplete));
        }
        
        private IEnumerator GetConversationsCoroutine(Action<List<ChatConversationData>, string> onComplete)
        {
            // Check if endpoints is available (requires environment config)
            if (endpoints == null)
            {
                Debug.LogWarning("API endpoints not available - falling back to cache");
                onComplete?.Invoke(null, "API not configured");
                yield break;
            }
            
            // Get organization and microgame IDs for Editor context
            if (!HasValidContextIds())
            {
                onComplete?.Invoke(null, "Organization ID and Microgame ID are required for Editor context");
                yield break;
            }
            
            if (!Guid.TryParse(organizationId, out var orgGuid))
            {
                onComplete?.Invoke(null, "Invalid Organization ID format");
                yield break;
            }
            
            if (!Guid.TryParse(microGameId, out var gameGuid))
            {
                onComplete?.Invoke(null, "Invalid Microgame ID format");
                yield break;
            }
            
            yield return endpoints.CoGetChatConversations((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null)
                {
                    var conversations = new List<ChatConversationData>();
                    
                    foreach (var conversationDto in dto.data)
                    {
                        var conversation = new ChatConversationData
                        {
                            id = conversationDto.id,
                            profileId = "", // Will need to be determined from relationships
                            createdAt = conversationDto.attributes.CreatedAt,
                            lastModified = conversationDto.attributes.UpdatedAt,
                            status = "synced"
                        };
                        
                        conversations.Add(conversation);
                        dataCache.AddConversation(conversation);
                    }
                    
                    Debug.Log($"Retrieved {conversations.Count} conversations from API");
                    onComplete?.Invoke(conversations, null);
                }
                else
                {
                    Debug.LogError($"Failed to get conversations: {result}");
                    onComplete?.Invoke(null, $"API Error: {result}");
                }
            },
            orgGuid,
            Guid.Empty, // userId - not needed for this call
            gameGuid);
        }
        
        /// <summary>
        /// Get conversations for a specific profile from cache
        /// </summary>
        public void GetConversationsForProfile(string profileId, Action<List<ChatConversationData>, string> onComplete)
        {
            try
            {
                var conversations = dataCache.GetConversationsForProfile(profileId);
                onComplete?.Invoke(conversations, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(null, ex.Message);
            }
        }
        
        /// <summary>
        /// Create a new conversation using real API or fallback to local
        /// </summary>
        public void CreateConversation(ChatConversationData conversation, Action<bool, string> onComplete)
        {
            if (endpoints == null)
            {
                // Fallback to local creation
                try
                {
                    if (conversation == null)
                    {
                        onComplete?.Invoke(false, "Conversation data is null");
                        return;
                    }
                    
                    conversation.status = "new";
                    dataCache.AddConversation(conversation);
                    
                    Debug.Log($"Conversation created locally for profile {conversation.profileId}");
                    onComplete?.Invoke(true, null);
                }
                catch (Exception ex)
                {
                    onComplete?.Invoke(false, ex.Message);
                }
                return;
            }
            
            coroutineRunner.StartCoroutine(CreateConversationCoroutine(conversation, onComplete));
        }
        
        private IEnumerator CreateConversationCoroutine(ChatConversationData conversation, Action<bool, string> onComplete)
        {
            // Check if endpoints is available (requires environment config)
            if (endpoints == null)
            {
                conversation.status = "local";
                dataCache.AddConversation(conversation);
                Debug.LogWarning($"Conversation saved locally - API endpoints not available");
                onComplete?.Invoke(true, "Conversation saved locally (API not configured)");
                yield break;
            }
            
            // Get organization and microgame IDs for Editor context
            if (!HasValidContextIds())
            {
                onComplete?.Invoke(false, "Organization ID and Microgame ID are required for Editor context");
                yield break;
            }
            
            if (!Guid.TryParse(organizationId, out var orgGuid))
            {
                onComplete?.Invoke(false, "Invalid Organization ID format");
                yield break;
            }
            
            if (!Guid.TryParse(microGameId, out var gameGuid))
            {
                onComplete?.Invoke(false, "Invalid Microgame ID format");
                yield break;
            }
            
            yield return endpoints.CoCreateChatConversation((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null)
                {
                    // Update the conversation with API response data
                    conversation.id = dto.data.id;
                    conversation.createdAt = dto.data.attributes.CreatedAt;
                    conversation.lastModified = dto.data.attributes.UpdatedAt;
                    conversation.status = "synced";
                    
                    dataCache.AddConversation(conversation);
                    Debug.Log($"Created conversation with ID: {conversation.id}");
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    Debug.LogError($"Failed to create conversation: {result}");
                    onComplete?.Invoke(false, $"API Error: {result}");
                }
            },
            orgGuid,
            Guid.Empty, // userId - will be determined by the API
            gameGuid);
        }
        
        /// <summary>
        /// Delete a conversation using real API or fallback to local
        /// </summary>
        public void DeleteConversation(string conversationId, Action<bool, string> onComplete)
        {
            if (endpoints == null)
            {
                // Fallback to local deletion
                try
                {
                    if (string.IsNullOrEmpty(conversationId))
                    {
                        onComplete?.Invoke(false, "Conversation ID is null or empty");
                        return;
                    }
                    
                    dataCache.RemoveConversation(conversationId);
                    
                    Debug.Log($"Conversation {conversationId} deleted locally");
                    onComplete?.Invoke(true, null);
                }
                catch (Exception ex)
                {
                    onComplete?.Invoke(false, ex.Message);
                }
                return;
            }
            
            coroutineRunner.StartCoroutine(DeleteConversationCoroutine(conversationId, onComplete));
        }
        
        private IEnumerator DeleteConversationCoroutine(string conversationId, Action<bool, string> onComplete)
        {
            if (!System.Guid.TryParse(conversationId, out System.Guid conversationGuid))
            {
                Debug.LogError($"Invalid conversation ID format: {conversationId}");
                onComplete?.Invoke(false, "Invalid conversation ID format");
                yield break;
            }
            
            yield return endpoints.CoDeleteChatConversation(conversationGuid, (result) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    dataCache.RemoveConversation(conversationId);
                    Debug.Log($"Deleted conversation with ID: {conversationId}");
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    Debug.LogError($"Failed to delete conversation: {result}");
                    onComplete?.Invoke(false, $"API Error: {result}");
                }
            });
        }
        
        #endregion
        
        #region Chat Messages
        
        /// <summary>
        /// Get messages for a specific conversation using real API or fallback to cache
        /// </summary>
        public void GetMessagesForConversation(string conversationId, Action<List<ChatMessageData>, string> onComplete)
        {
            if (endpoints == null)
            {
                // Fallback to cached data
                try
                {
                    var messages = dataCache.GetMessagesForConversation(conversationId);
                    onComplete?.Invoke(messages, null);
                }
                catch (Exception ex)
                {
                    onComplete?.Invoke(null, ex.Message);
                }
                return;
            }
            
            coroutineRunner.StartCoroutine(GetMessagesForConversationCoroutine(conversationId, onComplete));
        }
        
        private IEnumerator GetMessagesForConversationCoroutine(string conversationId, Action<List<ChatMessageData>, string> onComplete)
        {
            if (!System.Guid.TryParse(conversationId, out System.Guid conversationGuid))
            {
                Debug.LogError($"Invalid conversation ID format: {conversationId}");
                onComplete?.Invoke(null, "Invalid conversation ID format");
                yield break;
            }
            
            yield return endpoints.CoGetChatConversationMessages((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null)
                {
                    var messages = new List<ChatMessageData>();
                    
                    foreach (var messageDto in dto.data)
                    {
                        var message = new ChatMessageData
                        {
                            id = messageDto.id,
                            conversationId = conversationId,
                            role = messageDto.attributes.role,
                            content = messageDto.attributes.message,
                            createdAt = messageDto.attributes.CreatedAt,
                            lastModified = messageDto.attributes.UpdatedAt,
                            status = "synced"
                        };
                        
                        messages.Add(message);
                        dataCache.AddMessage(message);
                    }
                    
                    Debug.Log($"Retrieved {messages.Count} messages for conversation {conversationId}");
                    onComplete?.Invoke(messages, null);
                }
                else
                {
                    Debug.LogError($"Failed to get messages: {result}");
                    onComplete?.Invoke(null, $"API Error: {result}");
                }
            }, conversationGuid);
        }
        
        /// <summary>
        /// Create a new message using real API or fallback to local
        /// </summary>
        public void CreateMessage(ChatMessageData message, Action<bool, string> onComplete)
        {
            if (endpoints == null)
            {
                // Fallback to local creation
                try
                {
                    if (message == null)
                    {
                        onComplete?.Invoke(false, "Message data is null");
                        return;
                    }
                    
                    message.status = "new";
                    dataCache.AddMessage(message);
                    
                    Debug.Log($"Message created locally for conversation {message.conversationId}");
                    onComplete?.Invoke(true, null);
                }
                catch (Exception ex)
                {
                    onComplete?.Invoke(false, ex.Message);
                }
                return;
            }
            
            coroutineRunner.StartCoroutine(CreateMessageCoroutine(message, onComplete));
        }
        
        private IEnumerator CreateMessageCoroutine(ChatMessageData message, Action<bool, string> onComplete)
        {
            if (message == null)
            {
                onComplete?.Invoke(false, "Message data is null");
                yield break;
            }
            
            if (!System.Guid.TryParse(message.conversationId, out System.Guid conversationGuid))
            {
                Debug.LogError($"Invalid conversation ID format: {message.conversationId}");
                onComplete?.Invoke(false, "Invalid conversation ID format");
                yield break;
            }
            
            yield return endpoints.CoCreateChatConversationMessage(
                message.role,
                message.content,
                conversationGuid,
                (result, dto) =>
                {
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null)
                    {
                        // Update the message with API response data
                        message.id = dto.data.id;
                        message.createdAt = dto.data.attributes.CreatedAt;
                        message.lastModified = dto.data.attributes.UpdatedAt;
                        message.status = "synced";
                        
                        dataCache.AddMessage(message);
                        Debug.Log($"Created message with ID: {message.id}");
                        onComplete?.Invoke(true, null);
                    }
                    else
                    {
                        Debug.LogError($"Failed to create message: {result}");
                        onComplete?.Invoke(false, $"API Error: {result}");
                    }
                }
            );
        }
        
        /// <summary>
        /// Update an existing message locally
        /// </summary>
        public void UpdateMessage(ChatMessageData message, Action<bool, string> onComplete)
        {
            try
            {
                if (message == null)
                {
                    onComplete?.Invoke(false, "Message data is null");
                    return;
                }
                
                dataCache.UpdateMessage(message);
                
                Debug.Log($"Message updated locally");
                onComplete?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        /// <summary>
        /// Delete a message locally
        /// </summary>
        public void DeleteMessage(string messageId, Action<bool, string> onComplete)
        {
            try
            {
                if (string.IsNullOrEmpty(messageId))
                {
                    onComplete?.Invoke(false, "Message ID is null or empty");
                    return;
                }
                
                dataCache.RemoveMessage(messageId);
                
                Debug.Log($"Message {messageId} deleted locally");
                onComplete?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        #endregion
        
        #region Predefined Messages
        
        /// <summary>
        /// Get all predefined messages using real API or fallback to cache
        /// </summary>
        public void GetAllPredefinedMessages(Action<List<ChatPredefinedMessageData>, string> onComplete)
        {
            if (endpoints == null)
            {
                // Fallback to cached data
                try
                {
                    var messages = dataCache.GetAllPredefinedMessages();
                    onComplete?.Invoke(messages, null);
                }
                catch (Exception ex)
                {
                    onComplete?.Invoke(null, ex.Message);
                }
                return;
            }
            
            coroutineRunner.StartCoroutine(GetPredefinedMessagesCoroutine(onComplete));
        }
        
        private IEnumerator GetPredefinedMessagesCoroutine(Action<List<ChatPredefinedMessageData>, string> onComplete)
        {
            // Check if endpoints is available (requires environment config)
            if (endpoints == null)
            {
                Debug.LogWarning("API endpoints not available - falling back to cache");
                onComplete?.Invoke(null, "API not configured");
                yield break;
            }
            
            // Get organization and microgame IDs for Editor context
            if (!HasValidContextIds())
            {
                onComplete?.Invoke(null, "Organization ID and Microgame ID are required for Editor context");
                yield break;
            }
            
            if (!Guid.TryParse(organizationId, out var orgGuid))
            {
                onComplete?.Invoke(null, "Invalid Organization ID format");
                yield break;
            }
            
            if (!Guid.TryParse(microGameId, out var gameGuid))
            {
                onComplete?.Invoke(null, "Invalid Microgame ID format");
                yield break;
            }
            
            yield return endpoints.CoGetChatPredefinedMessages((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null)
                {
                    var messages = new List<ChatPredefinedMessageData>();
                    
                    foreach (var messageDto in dto.data)
                    {
                        var message = new ChatPredefinedMessageData
                        {
                            id = messageDto.id,
                            identifier = messageDto.attributes.identifier,
                            content = messageDto.attributes.content,
                            buttonName = messageDto.attributes.button_name,
                            createdAt = messageDto.attributes.CreatedAt,
                            lastModified = messageDto.attributes.UpdatedAt,
                            status = "synced"
                        };
                        
                        messages.Add(message);
                        dataCache.AddPredefinedMessage(message);
                    }
                    
                    Debug.Log($"Retrieved {messages.Count} predefined messages from API");
                    onComplete?.Invoke(messages, null);
                }
                else
                {
                    Debug.LogError($"Failed to get predefined messages: {result}");
                    onComplete?.Invoke(null, $"API Error: {result}");
                }
            },
            orgGuid,
            gameGuid);
        }
        
        /// <summary>
        /// Create a new predefined message locally
        /// </summary>
        public void CreatePredefinedMessage(ChatPredefinedMessageData message, Action<bool, string> onComplete)
        {
            try
            {
                if (message == null)
                {
                    onComplete?.Invoke(false, "Message data is null");
                    return;
                }
                
                message.status = "creating";
                dataCache.AddPredefinedMessage(message);
                
                // Use the real API endpoint to create predefined message
                if (endpoints != null)
                {
                    coroutineRunner.StartCoroutine(CreatePredefinedMessageCoroutine(message, onComplete));
                }
                else
                {
                    message.status = "local";
                    dataCache.UpdatePredefinedMessage(message);
                    Debug.LogWarning($"Predefined message '{message.identifier}' saved locally - API endpoints not available");
                    onComplete?.Invoke(true, "Message saved locally (API not configured)");
                }
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        private IEnumerator CreatePredefinedMessageCoroutine(ChatPredefinedMessageData message, Action<bool, string> onComplete)
        {
            // Check if endpoints is available (requires environment config)
            if (endpoints == null)
            {
                message.status = "local";
                dataCache.UpdatePredefinedMessage(message);
                Debug.LogWarning($"Predefined message '{message.identifier}' saved locally - API endpoints not available");
                onComplete?.Invoke(true, "Message saved locally (API not configured)");
                yield break;
            }
            
            // Validate that we have the required context IDs for Editor API calls
            if (!HasValidContextIds())
            {
                message.status = "error";
                dataCache.UpdatePredefinedMessage(message);
                Debug.LogError("Organization ID and Microgame ID must be set for Editor context API calls");
                onComplete?.Invoke(false, "Organization ID and Microgame ID are required for API calls in Editor context");
                yield break;
            }
            
            // Parse the IDs to Guid format
            if (!Guid.TryParse(organizationId, out Guid orgGuid) || !Guid.TryParse(microGameId, out Guid gameGuid))
            {
                message.status = "error";
                dataCache.UpdatePredefinedMessage(message);
                Debug.LogError($"Invalid GUID format - Organization ID: {organizationId}, Microgame ID: {microGameId}");
                onComplete?.Invoke(false, "Invalid GUID format for Organization ID or Microgame ID");
                yield break;
            }
            
            yield return endpoints.CoCreateChatPredefinedMessage(
                message.identifier,
                message.content,
                message.buttons,
                message.buttonName,
                (result, dto) =>
                {
                    if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null && dto.data != null)
                    {
                        // Update local message with server data
                        message.id = dto.data.id;
                        message.status = "synced";
                        message.createdAt = dto.data.attributes.CreatedAt;
                        message.lastModified = dto.data.attributes.UpdatedAt;
                        
                        dataCache.UpdatePredefinedMessage(message);
                        
                        Debug.Log($"Predefined message '{message.identifier}' created successfully on server");
                        onComplete?.Invoke(true, null);
                    }
                    else
                    {
                        // Keep local copy but mark as failed
                        message.status = "local";
                        dataCache.UpdatePredefinedMessage(message);
                        
                        Debug.LogError($"Failed to create predefined message on server: {result}");
                        onComplete?.Invoke(false, $"Failed to create message: {result}");
                    }
                },
                orgGuid,
                gameGuid);
        }
        
        /// <summary>
        /// Update an existing predefined message locally
        /// </summary>
        public void UpdatePredefinedMessage(ChatPredefinedMessageData message, Action<bool, string> onComplete)
        {
            try
            {
                if (message == null)
                {
                    onComplete?.Invoke(false, "Message data is null");
                    return;
                }
                
                dataCache.UpdatePredefinedMessage(message);
                
                Debug.Log($"Predefined message '{message.identifier}' updated locally");
                onComplete?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        /// <summary>
        /// Delete a predefined message locally
        /// </summary>
        public void DeletePredefinedMessage(string messageId, Action<bool, string> onComplete)
        {
            try
            {
                if (string.IsNullOrEmpty(messageId))
                {
                    onComplete?.Invoke(false, "Message ID is null or empty");
                    return;
                }
                
                var message = dataCache.GetPredefinedMessage(messageId);
                if (message == null)
                {
                    onComplete?.Invoke(false, "Message not found");
                    return;
                }
                
                dataCache.RemovePredefinedMessage(messageId);
                
                Debug.Log($"Predefined message '{message.identifier}' deleted locally");
                onComplete?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        #endregion
        
        #region Instructions
        
        /// <summary>
        /// Get all instructions using real API or fallback to cache
        /// </summary>
        public void GetAllInstructions(Action<List<ChatInstructionData>, string> onComplete)
        {
            if (endpoints == null)
            {
                // Fallback to cached data
                try
                {
                    var instructions = dataCache.GetAllInstructions();
                    onComplete?.Invoke(instructions, null);
                }
                catch (Exception ex)
                {
                    onComplete?.Invoke(null, ex.Message);
                }
                return;
            }
            
            coroutineRunner.StartCoroutine(GetInstructionsCoroutine(onComplete));
        }
        
        private IEnumerator GetInstructionsCoroutine(Action<List<ChatInstructionData>, string> onComplete)
        {
            // Check if endpoints is available (requires environment config)
            if (endpoints == null)
            {
                Debug.LogWarning("API endpoints not available - falling back to cache");
                onComplete?.Invoke(null, "API not configured");
                yield break;
            }
            
            // Get organization and microgame IDs for Editor context
            if (!HasValidContextIds())
            {
                onComplete?.Invoke(null, "Organization ID and Microgame ID are required for Editor context");
                yield break;
            }
            
            if (!Guid.TryParse(organizationId, out var orgGuid))
            {
                onComplete?.Invoke(null, "Invalid Organization ID format");
                yield break;
            }
            
            if (!Guid.TryParse(microGameId, out var gameGuid))
            {
                onComplete?.Invoke(null, "Invalid Microgame ID format");
                yield break;
            }
            
            yield return endpoints.CoGetChatInstructions((result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null)
                {
                    var instructions = new List<ChatInstructionData>();
                    
                    foreach (var instructionDto in dto.data)
                    {
                        var instruction = new ChatInstructionData
                        {
                            id = instructionDto.id,
                            identifier = instructionDto.attributes.identifier,
                            content = instructionDto.attributes.instruction,
                            createdAt = instructionDto.attributes.CreatedAt,
                            lastModified = instructionDto.attributes.UpdatedAt,
                            status = "synced"
                        };
                        
                        instructions.Add(instruction);
                        dataCache.AddInstruction(instruction);
                    }
                    
                    Debug.Log($"Retrieved {instructions.Count} instructions from API");
                    onComplete?.Invoke(instructions, null);
                }
                else
                {
                    Debug.LogError($"Failed to get instructions: {result}");
                    onComplete?.Invoke(null, $"API Error: {result}");
                }
            },
            orgGuid,
            gameGuid);
        }
        
        /// <summary>
        /// Create a new instruction locally
        /// </summary>
        public void CreateInstruction(ChatInstructionData instruction, Action<bool, string> onComplete)
        {
            try
            {
                if (instruction == null)
                {
                    onComplete?.Invoke(false, "Instruction data is null");
                    return;
                }
                
                instruction.status = "creating";
                dataCache.AddInstruction(instruction);
                
                // Use the real API endpoint to create instruction
                coroutineRunner.StartCoroutine(CreateInstructionCoroutine(instruction, onComplete));
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        private IEnumerator CreateInstructionCoroutine(ChatInstructionData instruction, Action<bool, string> onComplete)
        {
            // Check if endpoints is available (requires environment config)
            if (endpoints == null)
            {
                instruction.status = "local";
                dataCache.UpdateInstruction(instruction);
                Debug.LogWarning($"Instruction '{instruction.identifier}' saved locally - API endpoints not available");
                onComplete?.Invoke(true, "Instruction saved locally (API not configured)");
                yield break;
            }
            
            // Validate that we have the required context IDs for Editor API calls
            if (!HasValidContextIds())
            {
                instruction.status = "error";
                dataCache.UpdateInstruction(instruction);
                Debug.LogError("Organization ID and Microgame ID must be set for Editor context API calls");
                onComplete?.Invoke(false, "Organization ID and Microgame ID are required for API calls in Editor context");
                yield break;
            }
            
            // Parse the IDs to Guid format
            if (!Guid.TryParse(organizationId, out Guid orgGuid) || !Guid.TryParse(microGameId, out Guid gameGuid))
            {
                instruction.status = "error";
                dataCache.UpdateInstruction(instruction);
                Debug.LogError($"Invalid GUID format - Organization ID: {organizationId}, Microgame ID: {microGameId}");
                onComplete?.Invoke(false, "Invalid GUID format for Organization ID or Microgame ID");
                yield break;
            }
            
            yield return endpoints.CoCreateChatInstruction(
                instruction.identifier, 
                instruction.content, 
                (result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null && dto.data != null)
                {
                    // Update local instruction with server data
                    instruction.id = dto.data.id;
                    instruction.status = "synced";
                    instruction.createdAt = dto.data.attributes.CreatedAt;
                    instruction.lastModified = dto.data.attributes.UpdatedAt;
                    
                    dataCache.UpdateInstruction(instruction);
                    
                    Debug.Log($"Instruction '{instruction.identifier}' created successfully on server");
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    // Keep local copy but mark as failed
                    instruction.status = "local";
                    dataCache.UpdateInstruction(instruction);
                    
                    Debug.LogError($"Failed to create instruction on server: {result}");
                    onComplete?.Invoke(false, $"Failed to create instruction: {result}");
                }
            },
            orgGuid,
            gameGuid);
        }
        
        /// <summary>
        /// Update an existing instruction using real API
        /// </summary>
        public void UpdateInstruction(ChatInstructionData instruction, Action<bool, string> onComplete)
        {
            try
            {
                if (instruction == null)
                {
                    onComplete?.Invoke(false, "Instruction data is null");
                    return;
                }
                
                instruction.status = "syncing";
                instruction.lastModified = DateTime.UtcNow;
                dataCache.UpdateInstruction(instruction);
                
                // Use the real API endpoint to update instruction
                coroutineRunner.StartCoroutine(UpdateInstructionCoroutine(instruction, onComplete));
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        private IEnumerator UpdateInstructionCoroutine(ChatInstructionData instruction, Action<bool, string> onComplete)
        {
            // Parse the instruction ID as a GUID
            if (!Guid.TryParse(instruction.id, out Guid instructionGuid))
            {
                instruction.status = "local";
                dataCache.UpdateInstruction(instruction);
                onComplete?.Invoke(false, "Invalid instruction ID format");
                yield break;
            }
            
            yield return endpoints.CoUpdateChatInstruction(instructionGuid, instruction.content, (result, dto) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success && dto != null && dto.data != null)
                {
                    // Update local instruction with server data
                    instruction.status = "synced";
                    instruction.lastModified = dto.data.attributes.UpdatedAt;
                    
                    dataCache.UpdateInstruction(instruction);
                    
                    Debug.Log($"Instruction '{instruction.identifier}' updated successfully on server");
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    // Revert status on failure
                    instruction.status = "local";
                    dataCache.UpdateInstruction(instruction);
                    
                    Debug.LogError($"Failed to update instruction on server: {result}");
                    onComplete?.Invoke(false, $"Failed to update instruction: {result}");
                }
            });
        }
        
        /// <summary>
        /// Delete an instruction using real API
        /// </summary>
        public void DeleteInstruction(ChatInstructionData instruction, Action<bool, string> onComplete)
        {
            try
            {
                if (instruction == null)
                {
                    onComplete?.Invoke(false, "Instruction is null");
                    return;
                }
                
                // Use the real API endpoint to delete instruction
                coroutineRunner.StartCoroutine(DeleteInstructionCoroutine(instruction.id, instruction.identifier, onComplete));
            }
            catch (Exception ex)
            {
                onComplete?.Invoke(false, ex.Message);
            }
        }
        
        private IEnumerator DeleteInstructionCoroutine(string instructionId, string identifier, Action<bool, string> onComplete)
        {
            // Parse the instruction ID as a GUID
            if (!Guid.TryParse(instructionId, out Guid instructionGuid))
            {
                onComplete?.Invoke(false, "Invalid instruction ID format");
                yield break;
            }
            
            yield return endpoints.CoDeleteChatInstruction(instructionGuid, (result) =>
            {
                if (result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    // Remove from local cache on successful deletion
                    dataCache.RemoveInstruction(instructionId);
                    
                    Debug.Log($"Instruction '{identifier}' deleted successfully from server");
                    onComplete?.Invoke(true, null);
                }
                else
                {
                    Debug.LogError($"Failed to delete instruction from server: {result}");
                    onComplete?.Invoke(false, $"Failed to delete instruction: {result}");
                }
            });
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearCache()
        {
            dataCache.ClearAll();
            Debug.Log("Chat data cache cleared");
        }
        
        /// <summary>
        /// Get cache statistics
        /// </summary>
        public ChatDataStatistics GetCacheStatistics()
        {
            return dataCache.GetStatistics();
        }
        
        /// <summary>
        /// Create sample data for testing
        /// </summary>
        public void CreateSampleData()
        {
            // Create sample profiles
            var profile1 = new ChatProfileData("Alice Smith", "alice@example.com");
            var profile2 = new ChatProfileData("Bob Johnson", "bob@example.com");
            
            dataCache.AddProfile(profile1);
            dataCache.AddProfile(profile2);
            
            // Create sample conversations for Alice
            var conv1 = new ChatConversationData(profile1.id);
            var conv2 = new ChatConversationData(profile1.id);
            
            dataCache.AddConversation(conv1);
            dataCache.AddConversation(conv2);
            
            // Create sample conversations for Bob
            var conv3 = new ChatConversationData(profile2.id);
            
            dataCache.AddConversation(conv3);
            
            // Create sample messages
            var msg1 = new ChatMessageData(conv1.id, "user", "Hello, I need help with my account.");
            var msg2 = new ChatMessageData(conv1.id, "bot", "Hello! I'd be happy to help you with your account. What specific issue are you experiencing?");
            var msg3 = new ChatMessageData(conv1.id, "user", "I can't log in to my dashboard.");
            var msg4 = new ChatMessageData(conv1.id, "bot", "I understand you're having trouble logging in. Let me help you troubleshoot this issue.");
            
            var msg5 = new ChatMessageData(conv2.id, "user", "Can you help me with billing?");
            var msg6 = new ChatMessageData(conv2.id, "bot", "Of course! I can help you with billing questions. What would you like to know?");
            
            var msg7 = new ChatMessageData(conv3.id, "user", "Hi there!");
            var msg8 = new ChatMessageData(conv3.id, "bot", "Hello! How can I assist you today?");
            
            dataCache.AddMessage(msg1);
            dataCache.AddMessage(msg2);
            dataCache.AddMessage(msg3);
            dataCache.AddMessage(msg4);
            dataCache.AddMessage(msg5);
            dataCache.AddMessage(msg6);
            dataCache.AddMessage(msg7);
            dataCache.AddMessage(msg8);
            
            Debug.Log("Sample data created: 2 profiles, 3 conversations, 8 messages");
        }
        
        #endregion
    }
}