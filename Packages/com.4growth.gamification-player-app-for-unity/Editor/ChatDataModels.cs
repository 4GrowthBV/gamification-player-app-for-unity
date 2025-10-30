using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// Base class for all chat database entries
    /// </summary>
    [Serializable]
    public abstract class ChatDatabaseEntry
    {
        public string id;
        public DateTime createdAt;
        public DateTime lastModified;
        public string status = "new"; // "new", "synced", "modified", "deleted", "error"
        public bool isDirty => status == "modified" || status == "new";
    }
    
    /// <summary>
    /// Data model for chat profiles
    /// </summary>
    [Serializable]
    public class ChatProfileData : ChatDatabaseEntry
    {
        public string name;
        public string userId;
        public List<string> conversationIds = new List<string>();
        
        public ChatProfileData()
        {
            id = Guid.NewGuid().ToString();
            createdAt = DateTime.Now;
            lastModified = DateTime.Now;
        }
        
        public ChatProfileData(string profileName, string userIdValue) : this()
        {
            name = profileName;
            userId = userIdValue;
        }
    }
    
    /// <summary>
    /// Data model for chat conversations
    /// </summary>
    [Serializable]
    public class ChatConversationData : ChatDatabaseEntry
    {
        public string profileId;
        public List<string> messageIds = new List<string>();
        
        public ChatConversationData()
        {
            id = Guid.NewGuid().ToString();
            createdAt = DateTime.Now;
            lastModified = DateTime.Now;
        }
        
        public ChatConversationData(string profileIdValue) : this()
        {
            profileId = profileIdValue;
        }
    }
    
    /// <summary>
    /// Data model for chat messages
    /// </summary>
    [Serializable]
    public class ChatMessageData : ChatDatabaseEntry
    {
        public string conversationId;
        public string role; // "user" or "bot"
        public string content;
        
        public ChatMessageData()
        {
            id = Guid.NewGuid().ToString();
            createdAt = DateTime.Now;
            lastModified = DateTime.Now;
        }
        
        public ChatMessageData(string conversationIdValue, string roleValue, string contentValue) : this()
        {
            conversationId = conversationIdValue;
            role = roleValue;
            content = contentValue;
        }
    }
    
    /// <summary>
    /// Data model for predefined messages
    /// </summary>
    [Serializable]
    public class ChatPredefinedMessageData : ChatDatabaseEntry
    {
        public string identifier;
        public string content;
        public List<string> buttons = new List<string>();
        public string buttonName;
        
        public ChatPredefinedMessageData()
        {
            id = Guid.NewGuid().ToString();
            createdAt = DateTime.Now;
            lastModified = DateTime.Now;
        }
        
        public ChatPredefinedMessageData(string identifierValue, string contentValue) : this()
        {
            identifier = identifierValue;
            content = contentValue;
        }
    }
    
    /// <summary>
    /// Data model for chat instructions
    /// </summary>
    [Serializable]
    public class ChatInstructionData : ChatDatabaseEntry
    {
        public string identifier;
        public string content;
        public string agent; // Agent type this instruction is for
        
        public ChatInstructionData()
        {
            id = Guid.NewGuid().ToString();
            createdAt = DateTime.Now;
            lastModified = DateTime.Now;
        }
        
        public ChatInstructionData(string identifierValue, string contentValue) : this()
        {
            identifier = identifierValue;
            content = contentValue;
        }
    }
    
    /// <summary>
    /// Statistics about cached data
    /// </summary>
    [Serializable]
    public struct ChatDataStatistics
    {
        public int profileCount;
        public int conversationCount;
        public int messageCount;
        public int predefinedMessageCount;
        public int instructionCount;
        public int unsavedCount;
        public int totalCount;
    }
    
    /// <summary>
    /// Cache system for chat database data
    /// Manages in-memory storage and tracks changes for synchronization
    /// </summary>
    public class ChatDataCache
    {
        #region Private Fields
        
        private Dictionary<string, ChatProfileData> profiles = new Dictionary<string, ChatProfileData>();
        private Dictionary<string, ChatConversationData> conversations = new Dictionary<string, ChatConversationData>();
        private Dictionary<string, ChatMessageData> messages = new Dictionary<string, ChatMessageData>();
        private Dictionary<string, ChatPredefinedMessageData> predefinedMessages = new Dictionary<string, ChatPredefinedMessageData>();
        private Dictionary<string, ChatInstructionData> instructions = new Dictionary<string, ChatInstructionData>();
        
        // Index for quick lookups
        private Dictionary<string, List<string>> profileToConversations = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> conversationToMessages = new Dictionary<string, List<string>>();
        
        #endregion
        
        #region Chat Profiles
        
        public void AddProfile(ChatProfileData profile)
        {
            if (profile == null) return;

            Debug.Log($"Adding profile: {profile.id}");
            
            profiles[profile.id] = profile;
            if (!profileToConversations.ContainsKey(profile.id))
            {
                profileToConversations[profile.id] = new List<string>();
            }
        }
        
        public void UpdateProfile(ChatProfileData profile)
        {
            if (profile == null) return;

            Debug.Log($"Updating profile: {profile.id}");

            profile.lastModified = DateTime.Now;
            profile.status = "modified";
            profiles[profile.id] = profile;
        }
        
        public void RemoveProfile(string profileId)
        {
            if (profiles.ContainsKey(profileId))
            {
                profiles.Remove(profileId);
                profileToConversations.Remove(profileId);
            }
        }
        
        public ChatProfileData GetProfile(string profileId)
        {
            return profiles.TryGetValue(profileId, out var profile) ? profile : null;
        }
        
        public List<ChatProfileData> GetAllProfiles()
        {
            return profiles.Values.ToList();
        }
        
        public List<ChatProfileData> SearchProfiles(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return GetAllProfiles();
                
            return profiles.Values
                .Where(p => p.name.ToLower().Contains(searchTerm.ToLower()) ||
                           p.userId.ToLower().Contains(searchTerm.ToLower()))
                .ToList();
        }
        
        #endregion
        
        #region Chat Conversations
        
        public void AddConversation(ChatConversationData conversation)
        {
            if (conversation == null) return;
            
            conversations[conversation.id] = conversation;
            
            // Update profile index
            if (!string.IsNullOrEmpty(conversation.profileId))
            {
                if (!profileToConversations.ContainsKey(conversation.profileId))
                {
                    profileToConversations[conversation.profileId] = new List<string>();
                }
                
                if (!profileToConversations[conversation.profileId].Contains(conversation.id))
                {
                    profileToConversations[conversation.profileId].Add(conversation.id);
                }
            }
            
            // Initialize message index
            if (!conversationToMessages.ContainsKey(conversation.id))
            {
                conversationToMessages[conversation.id] = new List<string>();
            }
        }
        
        public void RemoveConversation(string conversationId)
        {
            if (conversations.TryGetValue(conversationId, out var conversation))
            {
                // Remove from profile index
                if (!string.IsNullOrEmpty(conversation.profileId) && 
                    profileToConversations.TryGetValue(conversation.profileId, out var list))
                {
                    list.Remove(conversationId);
                }
                
                // Remove messages
                if (conversationToMessages.TryGetValue(conversationId, out var messageIds))
                {
                    foreach (var messageId in messageIds.ToList())
                    {
                        RemoveMessage(messageId);
                    }
                }
                
                conversations.Remove(conversationId);
                conversationToMessages.Remove(conversationId);
            }
        }
        
        public ChatConversationData GetConversation(string conversationId)
        {
            return conversations.TryGetValue(conversationId, out var conversation) ? conversation : null;
        }
        
        public List<ChatConversationData> GetConversationsForProfile(string profileId)
        {
            if (profileToConversations.TryGetValue(profileId, out var conversationIds))
            {
                return conversationIds.Select(id => conversations.TryGetValue(id, out var conv) ? conv : null)
                                    .Where(conv => conv != null)
                                    .ToList();
            }
            return new List<ChatConversationData>();
        }
        
        public void SetConversations(string profileId, List<ChatConversationData> conversationList)
        {
            foreach (var conversation in conversationList)
            {
                conversation.profileId = profileId;
                AddConversation(conversation);
            }
        }
        
        #endregion
        
        #region Chat Messages
        
        public void AddMessage(ChatMessageData message)
        {
            if (message == null) return;
            
            messages[message.id] = message;
            
            // Update conversation index
            if (!string.IsNullOrEmpty(message.conversationId))
            {
                if (!conversationToMessages.ContainsKey(message.conversationId))
                {
                    conversationToMessages[message.conversationId] = new List<string>();
                }
                
                if (!conversationToMessages[message.conversationId].Contains(message.id))
                {
                    conversationToMessages[message.conversationId].Add(message.id);
                }
            }
        }
        
        public void UpdateMessage(ChatMessageData message)
        {
            if (message == null) return;
            
            message.lastModified = DateTime.Now;
            message.status = "modified";
            messages[message.id] = message;
        }
        
        public void RemoveMessage(string messageId)
        {
            if (messages.TryGetValue(messageId, out var message))
            {
                // Remove from conversation index
                if (!string.IsNullOrEmpty(message.conversationId) &&
                    conversationToMessages.TryGetValue(message.conversationId, out var list))
                {
                    list.Remove(messageId);
                }
                
                messages.Remove(messageId);
            }
        }
        
        public ChatMessageData GetMessage(string messageId)
        {
            return messages.TryGetValue(messageId, out var message) ? message : null;
        }
        
        public List<ChatMessageData> GetMessagesForConversation(string conversationId)
        {
            if (conversationToMessages.TryGetValue(conversationId, out var messageIds))
            {
                return messageIds.Select(id => messages.TryGetValue(id, out var msg) ? msg : null)
                                .Where(msg => msg != null)
                                .OrderBy(msg => msg.createdAt)
                                .ToList();
            }
            return new List<ChatMessageData>();
        }
        
        public void SetMessages(string conversationId, List<ChatMessageData> messageList)
        {
            foreach (var message in messageList)
            {
                message.conversationId = conversationId;
                AddMessage(message);
            }
        }
        
        #endregion
        
        #region Predefined Messages
        
        public void AddPredefinedMessage(ChatPredefinedMessageData message)
        {
            if (message == null) return;
            predefinedMessages[message.identifier] = message;
        }
        
        public void UpdatePredefinedMessage(ChatPredefinedMessageData message)
        {
            if (message == null) return;
            
            message.lastModified = DateTime.Now;
            message.status = "modified";
            predefinedMessages[message.identifier] = message;
        }
        
        public void RemovePredefinedMessage(string messageIdentifier)
        {
            predefinedMessages.Remove(messageIdentifier);
        }
        
        public ChatPredefinedMessageData GetPredefinedMessage(string messageIdentifier)
        {
            return predefinedMessages.TryGetValue(messageIdentifier, out var message) ? message : null;
        }
        
        public List<ChatPredefinedMessageData> GetAllPredefinedMessages()
        {
            return predefinedMessages.Values.ToList();
        }
        
        public void SetPredefinedMessages(List<ChatPredefinedMessageData> messageList)
        {
            predefinedMessages.Clear();
            foreach (var message in messageList)
            {
                AddPredefinedMessage(message);
            }
        }
        
        #endregion
        
        #region Instructions
        
        public void AddInstruction(ChatInstructionData instruction)
        {
            if (instruction == null) return;
            instructions[instruction.identifier] = instruction;
        }
        
        public void UpdateInstruction(ChatInstructionData instruction)
        {
            if (instruction == null) return;
            
            instruction.lastModified = DateTime.Now;
            instruction.status = "modified";
            instructions[instruction.identifier] = instruction;
        }
        
        public void RemoveInstruction(string instructionId)
        {
            instructions.Remove(instructionId);
        }
        
        public ChatInstructionData GetInstruction(string instructionId)
        {
            return instructions.TryGetValue(instructionId, out var instruction) ? instruction : null;
        }
        
        public List<ChatInstructionData> GetAllInstructions()
        {
            foreach (var kvp in instructions)
            {
                Debug.Log($"Instruction: {kvp.Key} - {kvp.Value.identifier}");
            }

            return instructions.Values.ToList();
        }
        
        public void SetInstructions(List<ChatInstructionData> instructionList)
        {
            instructions.Clear();
            foreach (var instruction in instructionList)
            {
                AddInstruction(instruction);
            }
        }
        
        #endregion
        
        #region Cache Management
        
        public bool HasUnsavedChanges()
        {
            return profiles.Values.Any(p => p.isDirty) ||
                   conversations.Values.Any(c => c.isDirty) ||
                   messages.Values.Any(m => m.isDirty) ||
                   predefinedMessages.Values.Any(pm => pm.isDirty) ||
                   instructions.Values.Any(i => i.isDirty);
        }
        
        public ChatDataStatistics GetStatistics()
        {
            var unsavedCount = profiles.Values.Count(p => p.isDirty) +
                              conversations.Values.Count(c => c.isDirty) +
                              messages.Values.Count(m => m.isDirty) +
                              predefinedMessages.Values.Count(pm => pm.isDirty) +
                              instructions.Values.Count(i => i.isDirty);
            
            return new ChatDataStatistics
            {
                profileCount = profiles.Count,
                conversationCount = conversations.Count,
                messageCount = messages.Count,
                predefinedMessageCount = predefinedMessages.Count,
                instructionCount = instructions.Count,
                unsavedCount = unsavedCount,
                totalCount = profiles.Count + conversations.Count + messages.Count + 
                           predefinedMessages.Count + instructions.Count
            };
        }
        
        public void ClearAll()
        {
            profiles.Clear();
            conversations.Clear();
            messages.Clear();
            predefinedMessages.Clear();
            instructions.Clear();
            profileToConversations.Clear();
            conversationToMessages.Clear();
        }
        
        public void MarkAllAsSynced()
        {
            foreach (var profile in profiles.Values)
                profile.status = "synced";
            foreach (var conversation in conversations.Values)
                conversation.status = "synced";
            foreach (var message in messages.Values)
                message.status = "synced";
            foreach (var predefinedMessage in predefinedMessages.Values)
                predefinedMessage.status = "synced";
            foreach (var instruction in instructions.Values)
                instruction.status = "synced";
        }
        
        #endregion
        
        #region Search & Filter
        
        public List<T> SearchEntries<T>(string searchTerm, List<T> entries) where T : ChatDatabaseEntry
        {
            if (string.IsNullOrEmpty(searchTerm))
                return entries;
            
            var searchLower = searchTerm.ToLower();
            return entries.Where(entry =>
            {
                switch (entry)
                {
                    case ChatProfileData profile:
                        return profile.name.ToLower().Contains(searchLower) ||
                               profile.userId.ToLower().Contains(searchLower);
                    case ChatMessageData message:
                        return message.content.ToLower().Contains(searchLower) ||
                               message.role.ToLower().Contains(searchLower);
                    case ChatPredefinedMessageData predefined:
                        return predefined.identifier.ToLower().Contains(searchLower) ||
                               predefined.content.ToLower().Contains(searchLower);
                    case ChatInstructionData instruction:
                        return instruction.identifier.ToLower().Contains(searchLower) ||
                               instruction.content.ToLower().Contains(searchLower);
                    default:
                        return entry.id.ToLower().Contains(searchLower);
                }
            }).ToList();
        }
        
        #endregion
    }
}