// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System;
using System.Collections.Generic;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.ModuleSession
{
    public class GetModuleSessionResponseDTO
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

            public DateTime? CompletedAt
            {
                get
                {
                    if(string.IsNullOrEmpty(completed_at))
                    {
                        return null;
                    }

                    return DateTime.Parse(completed_at);
                }
            }
            
            public string started_at;
            
            #nullable enable
            public string? ended_at;
            public string? completed_at;
            #nullable disable

            public int score;
            public Dictionary<string, string> extra_data;
        }

        [Serializable]
        public class ChallengeSession
        {
            [Serializable]
            public class Data : ILoggableData
            {
                public string Type { get => type; }
                public float Time { get; set; }

                public string id;
                public string type;
            }

            public Data data;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            public string id;
            public string type;
            public Attributes attributes;
            public Relationships relationships;
        }

        [Serializable]
        public class Module
        {
            [Serializable]
            public class Data : ILoggableData
            {
                public string Type { get => type; }
                public float Time { get; set; }

                [ModuleId]
                public string id;
                public string type;
            }
            
            public Data data;
        }

        [Serializable]
        public class Relationships
        {
            public ChallengeSession challenge_session;
            public Module module;
        }

        public Data data;
    }
}