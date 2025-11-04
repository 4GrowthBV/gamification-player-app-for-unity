using System;
using System.Collections.Generic;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.Chat
{
    public class GetChatPredefinedMessageResponseDTO
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

            public string identifier;
            public string content;
            public List<string> buttons;
            public string button_name;
            public string created_at;
            public string updated_at;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

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
            public Dictionary<string, object> attributes;
        }

        public Data data;
        public List<IncludedItem> included;
    }
}