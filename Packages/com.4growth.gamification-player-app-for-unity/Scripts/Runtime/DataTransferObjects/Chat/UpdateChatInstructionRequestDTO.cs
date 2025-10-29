using System;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for updating a chat instruction.
    /// </summary>
    public class UpdateChatInstructionRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization

        public UpdateChatInstructionRequestDTO()

        {

        }

        

        private UpdateChatInstructionRequestDTO(DataContainer data)
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

            public float Time { get; set; }

            // Default constructor for JSON deserialization

            public DataContainer()

            {

            }

            

            public DataContainer(AttributesContainer attributes)
            {
                Attributes = attributes;
            }
        }

        /// <summary>
        /// Inner class for attributes.
        /// </summary>
        public class AttributesContainer
        {
            [JsonProperty("instruction")]
            public string Instruction { get; set; }

            // Default constructor for JSON deserialization

            public AttributesContainer()

            {

            }

            

            public AttributesContainer(string instruction)
            {
                Instruction = instruction;
            }
        }

        /// <summary>
        /// Creates a request for updating a chat instruction.
        /// </summary>
        public static UpdateChatInstructionRequestDTO Create(string instruction)
        {
            var attributes = new AttributesContainer(instruction);
            return new UpdateChatInstructionRequestDTO(new DataContainer(attributes));
        }
    }
}

