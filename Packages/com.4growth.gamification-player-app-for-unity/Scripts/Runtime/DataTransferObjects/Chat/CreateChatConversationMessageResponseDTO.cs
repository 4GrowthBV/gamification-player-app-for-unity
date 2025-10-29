using System;
using System.Collections.Generic;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.Chat
{
    public class CreateChatConversationMessageResponseDTO
    {
        [Serializable]
        public class MessageAttributes
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

            [ChatRole]
            public string role;
            
            [ChatMessage]
            public string message;
            
            public string created_at;
            public string updated_at;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            [ChatConversationMessageId]
            public string id;
            
            public string type;
            public MessageAttributes attributes;
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