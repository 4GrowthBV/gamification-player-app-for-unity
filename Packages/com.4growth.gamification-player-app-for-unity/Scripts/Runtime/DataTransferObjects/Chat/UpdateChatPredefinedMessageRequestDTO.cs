using System;
using System.Collections.Generic;
using System.Globalization;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.Chat
{
    /// <summary>
    /// Represents the request body for updating a chat predefined message.
    /// </summary>
    public class UpdateChatPredefinedMessageRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; set; }

        // Default constructor for JSON deserialization

        public UpdateChatPredefinedMessageRequestDTO()

        {

        }

        

        private UpdateChatPredefinedMessageRequestDTO(DataContainer data)
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

            

            public AttributesContainer(string content, List<string> buttons, string buttonName)
            {
                Content = content;
                Buttons = buttons;
                ButtonName = buttonName;
            }
        }

        /// <summary>
        /// Creates a request for updating a chat predefined message.
        /// </summary>
        public static UpdateChatPredefinedMessageRequestDTO Create(
            string content,
            List<string> buttons,
            string buttonName)
        {
            var attributes = new AttributesContainer(content, buttons, buttonName);
            return new UpdateChatPredefinedMessageRequestDTO(new DataContainer(attributes));
        }
    }
}

