using System;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for creating a chat instruction.
    /// </summary>
    public class CreateChatInstructionRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization

        public CreateChatInstructionRequestDTO()

        {

        }

        

        private CreateChatInstructionRequestDTO(DataContainer data)
        {
            Data = data;
        }

        /// <summary>
        /// Inner class matching the schema's "data" object.
        /// </summary>
        public class DataContainer : ILoggableData
        {
            [JsonProperty("type")]
            public string Type => "chat_instruction";

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

            [JsonProperty("instruction")]
            public string Instruction { get; set; }

            // Default constructor for JSON deserialization

            public AttributesContainer()

            {

            }

            

            public AttributesContainer(string identifier, string instruction)
            {
                Identifier = identifier;
                Instruction = instruction;
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
        /// Creates a request for creating a chat instruction.
        /// </summary>
        public static CreateChatInstructionRequestDTO Create(
            string identifier,
            string instruction,
            Guid organisationId,
            Guid microGameId)
        {
            var attributes = new AttributesContainer(identifier, instruction);
            var relationships = new RelationshipsContainer(organisationId, microGameId);
            return new CreateChatInstructionRequestDTO(new DataContainer(attributes, relationships));
        }
    }
}

