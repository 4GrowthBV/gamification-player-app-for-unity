using System;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.TakeAway
{
    public class TakeAwaySessionResponseDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime StartedAt
            {
                get
                {
                    return DateTime.Parse(started_at);
                }
            }

            public DateTime? EndedAt
            {
                get
                {
                    if(string.IsNullOrEmpty(ended_at))
                    {
                        return null;
                    }

                    return DateTime.Parse(ended_at);
                }
            }

            public string started_at;

            #nullable enable
            public string? ended_at;
            #nullable disable
        }

        [Serializable]
        public class Relationships
        {
            [Serializable]
            public class Type
            {
                [Serializable]
                public class Data
                {
                    public string type;
                    public string id;
                }

                public Data data;
            }

            public Type micro_game;
            public Type user;
            public Type organisation;
            public Type module_session;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            [TakeAwaySessionId]
            public string id;
            public string type;

            public Attributes attributes;
            public Relationships relationships;
        }

        public Data data;
    }
}
