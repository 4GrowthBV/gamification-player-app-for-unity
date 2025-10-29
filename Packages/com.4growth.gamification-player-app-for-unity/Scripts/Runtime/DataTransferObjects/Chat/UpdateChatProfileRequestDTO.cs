using System;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for updating a chat profile.
    /// </summary>
    public class UpdateChatProfileRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization

        public UpdateChatProfileRequestDTO()

        {

        }

        

        private UpdateChatProfileRequestDTO(DataContainer data)
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
        /// Creates a request for updating a chat profile.
        /// </summary>
        public static UpdateChatProfileRequestDTO Create(string profile)
        {
            var attributes = new AttributesContainer(profile);
            return new UpdateChatProfileRequestDTO(new DataContainer(attributes));
        }
    }
}

