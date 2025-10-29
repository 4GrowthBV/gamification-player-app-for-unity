using System;
using System.Collections.Generic;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.Chat
{
    public class CreateChatPredefinedMessageResponseDTO
    {
        [Serializable]
        public class PredefinedMessageAttributes
        {
            public DateTime CreatedAt
            {
                get
                {
                    return DateTime.Parse(created_at);
                }
            }

            public DateTime UpdatedAt
            {
                get
                {
                    return DateTime.Parse(updated_at);
                }
            }

            [ChatPredefinedMessageIdentifier]
            public string identifier;
            
            [ChatPredefinedMessageContent]
            public string content;
            
            [ChatPredefinedMessageButtons]
            public string[] buttons;
            
            [ChatPredefinedMessageButtonName]
            public string button_name;
            
            public string created_at;
            public string updated_at;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            [ChatPredefinedMessageId]
            public string id;
            
            public string type;
            public PredefinedMessageAttributes attributes;
        }

        [Serializable]
        public class IncludedItem : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            public string id;
            public string type;
            public object attributes;
        }

        public Data data;
        public List<IncludedItem> included;
    }
}