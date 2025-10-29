using System;
using System.Collections.Generic;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for creating a chat predefined message.
    /// </summary>
    public class CreateChatPredefinedMessageRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization
        public CreateChatPredefinedMessageRequestDTO()
        {
        }

        private CreateChatPredefinedMessageRequestDTO(DataContainer data)
        {
            Data = data;
        }

        /// <summary>
        /// Inner class matching the schema's "data" object.
        /// </summary>
        public class DataContainer : ILoggableData
        {
            [JsonProperty("type")]
            public string Type => "chat_predefined_message";

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
            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }

            [JsonProperty("buttons")]
            public List<string> Buttons { get; set; }

            [JsonProperty("button_name")]
            public string ButtonName { get; set; }

            // Default constructor for JSON deserialization

            public AttributesContainer()

            {

            }

            

            public AttributesContainer(string identifier, string content, List<string> buttons, string buttonName)
            {
                Identifier = identifier;
                Content = content;
                Buttons = buttons;
                ButtonName = buttonName;
            }
        }

        /// <summary>
        /// Inner class for relationships.
        /// </summary>
        public class RelationshipsContainer
        {
            [JsonProperty("organisation")]
            public RelationshipData Organisation { get; set; }

            [JsonProperty("micro_game")]
            public RelationshipData MicroGame { get; set; }

            // Default constructor for JSON deserialization

            public RelationshipsContainer()

            {

            }

            

            public RelationshipsContainer(Guid organisationId, Guid microGameId)
            {
                Organisation = new RelationshipData("organisation", organisationId);
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
        /// Creates a request for creating a chat predefined message.
        /// </summary>
        public static CreateChatPredefinedMessageRequestDTO Create(
            string identifier,
            string content,
            List<string> buttons,
            string buttonName,
            Guid organisationId,
            Guid microGameId)
        {
            var attributes = new AttributesContainer(identifier, content, buttons, buttonName);
            var relationships = new RelationshipsContainer(organisationId, microGameId);
            return new CreateChatPredefinedMessageRequestDTO(new DataContainer(attributes, relationships));
        }
    }
}
