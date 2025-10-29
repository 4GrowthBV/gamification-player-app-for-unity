using System;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for creating a chat conversation.
    /// </summary>
    public class CreateChatConversationRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization
        public CreateChatConversationRequestDTO()
        {
        }

        private CreateChatConversationRequestDTO(DataContainer data)
        {
            Data = data;
        }

        /// <summary>
        /// Inner class matching the schema's "data" object.
        /// </summary>
        public class DataContainer : ILoggableData
        {
            [JsonProperty("type")]
            public string Type => "chat_conversation";

            [JsonProperty("relationships")]
            public RelationshipsContainer Relationships { get; set; }

            public float Time { get; set; }

            // Default constructor for JSON deserialization
            public DataContainer()
            {
            }

            public DataContainer(RelationshipsContainer relationships)
            {
                Relationships = relationships;
            }
        }

        /// <summary>
        /// Inner class for relationships.
        /// </summary>
        public class RelationshipsContainer
        {
            [JsonProperty("organisation")]
            public RelationshipData Organisation { get; set; }

            [JsonProperty("user")]
            public RelationshipData User { get; set; }

            [JsonProperty("micro_game")]
            public RelationshipData MicroGame { get; set; }

            // Default constructor for JSON deserialization
            public RelationshipsContainer()
            {
            }

            public RelationshipsContainer(Guid organisationId, Guid userId, Guid microGameId)
            {
                Organisation = new RelationshipData("organisation", organisationId);
                User = new RelationshipData("user", userId);
                MicroGame = new RelationshipData("micro_game", microGameId);
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
        /// Creates a request for creating a chat conversation.
        /// </summary>
        public static CreateChatConversationRequestDTO Create(
            Guid organisationId,
            Guid userId,
            Guid microGameId)
        {
            var relationships = new RelationshipsContainer(organisationId, userId, microGameId);
            return new CreateChatConversationRequestDTO(new DataContainer(relationships));
        }
    }
}