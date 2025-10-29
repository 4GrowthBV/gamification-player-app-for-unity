using System;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for creating a chat profile.
    /// </summary>
    public class CreateChatProfileRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization
        public CreateChatProfileRequestDTO()
        {
        }

        private CreateChatProfileRequestDTO(DataContainer data)
        {
            Data = data;
        }

        /// <summary>
        /// Inner class matching the schema's "data" object.
        /// </summary>
        public class DataContainer : ILoggableData
        {
            [JsonProperty("type")]
            public string Type => "chat_profile";

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
            [JsonProperty("profile")]
            public string Profile { get; set; }

            // Default constructor for JSON deserialization
            public AttributesContainer()
            {
            }

            public AttributesContainer(string profile)
            {
                Profile = profile;
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
        /// Creates a request for creating a chat profile.
        /// </summary>
        public static CreateChatProfileRequestDTO Create(
            string profile,
            Guid chatConversationId)
        {
            var attributes = new AttributesContainer(profile);
            var relationships = new RelationshipsContainer(chatConversationId);
            return new CreateChatProfileRequestDTO(new DataContainer(attributes, relationships));
        }
    }
}
