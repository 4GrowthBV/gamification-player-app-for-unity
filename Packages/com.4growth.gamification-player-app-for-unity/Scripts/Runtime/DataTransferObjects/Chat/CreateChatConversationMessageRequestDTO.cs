using System;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for creating a chat conversation message.
    /// </summary>
    public class CreateChatConversationMessageRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization
        public CreateChatConversationMessageRequestDTO()
        {
        }

        private CreateChatConversationMessageRequestDTO(DataContainer data)
        {
            Data = data;
        }

        /// <summary>
        /// Inner class matching the schema's "data" object.
        /// </summary>
        public class DataContainer : ILoggableData
        {
            [JsonProperty("type")]
            public string Type => "chat_conversation_message";

            [JsonProperty("attributes")]
            public AttributesContainer Attributes { get; set; }

            [JsonProperty("relationships")]
            public RelationshipsContainer Relationships { get; set; }

            public float Time { get; set; }

            // Default constructor for JSON deserialization
            public DataContainer()
            {
            }

            public DataContainer(AttributesContainer attributes, RelationshipsContainer relationships)
            {
                Attributes = attributes;
                Relationships = relationships;
            }
        }

        /// <summary>
        /// Inner class for attributes.
        /// </summary>
        public class AttributesContainer
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            // Default constructor for JSON deserialization
            public AttributesContainer()
            {
            }

            public AttributesContainer(string role, string message)
            {
                Role = role;
                Message = message;
            }
        }

        /// <summary>
        /// Inner class for relationships.
        /// </summary>
        public class RelationshipsContainer
        {
            [JsonProperty("chat_conversation")]
            public RelationshipData ChatConversation { get; set; }

            // Default constructor for JSON deserialization
            public RelationshipsContainer()
            {
            }

            public RelationshipsContainer(Guid chatConversationId)
            {
                ChatConversation = new RelationshipData("chat_conversation", chatConversationId);
            }
        }

        /// <summary>
        /// Relationship data structure.
        /// </summary>
        public class RelationshipData
        {
            [JsonProperty("data")]
            public RelationshipReference Data { get; set; }

            // Default constructor for JSON deserialization
            public RelationshipData()
            {
            }

            public RelationshipData(string type, Guid id)
            {
                Data = new RelationshipReference(type, id);
            }
        }

        /// <summary>
        /// Relationship reference structure.
        /// </summary>
        public class RelationshipReference
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            // Default constructor for JSON deserialization
            public RelationshipReference()
            {
            }

            public RelationshipReference(string type, Guid id)
            {
                Type = type;
                Id = id.ToString();
            }
        }

        /// <summary>
        /// Creates a request for creating a chat conversation message.
        /// </summary>
        public static CreateChatConversationMessageRequestDTO Create(
            string role,
            string message,
            Guid chatConversationId)
        {
            var attributes = new AttributesContainer(role, message);
            var relationships = new RelationshipsContainer(chatConversationId);
            return new CreateChatConversationMessageRequestDTO(new DataContainer(attributes, relationships));
        }
    }
}