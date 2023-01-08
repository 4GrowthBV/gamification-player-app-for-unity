using System;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.ChallengeSession
{
    public class UpdateChallengeSessionResponseDTO
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
        }

        [Serializable]
        public class Challenge
        {
            [Serializable]
            public class Data
            {
                public string Type { get => type; }
                
                [ChallengeId]
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
            
            [ChallengeSessionId]
            public string id;
            public string type;
            public Attributes attributes;
            public Relationships relationships;            
        }

        [Serializable]
        public class Relationships
        {
            public User user;
            public Challenge challenge;
        }

        [Serializable]
        public class User
        {
            [Serializable]
            public class Data
            {
                public string Type { get => type; }
                
                [UserId]
                public string id;
                public string type;     
            }

            public Data data;
        }

        public Data data;
    }
}