using System;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.ModuleSession
{
    public class UpdateModuleSessionResponseDTO
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

            [ModuleSessionStarted]
            public string started_at;

            #nullable enable
            [ModuleSessionEnded]
            public string? ended_at;
            
            [ModuleSessionCompleted]
            public string? completed_at;
            #nullable disable

            public int score;
        }

        [Serializable]
        public class ChallengeSession
        {
            [Serializable]
            public class Data
            {
                public string Type { get => type; }

                [ChallengeSessionId]
                public string id;
                public string type;  
            }

            public Data data;

            public ChallengeSession()
            {
                data = new Data();
            }
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            [ModuleSessionId]
            public string id;
            public string type;
            public Attributes attributes;
            public Relationships relationships;

            public Data()
            {
                attributes = new Attributes();
                relationships = new Relationships();
            }     
        }

        [Serializable]
        public class Relationships
        {
            public Module module;
            public ChallengeSession challenge_session;

            public Relationships()
            {
                module = new Module();
                challenge_session = new ChallengeSession();
            }
        }

        [Serializable]
        public class Module
        {
            [Serializable]
            public class Data
            {
                public string Type { get => type; }

                [ModuleId]
                public string id;
                public string type;  
            }

            public Data data;

            public Module()
            {
                data = new Module.Data();
            }
        }
        
        public Data data;

        public UpdateModuleSessionResponseDTO()
        {
            data = new Data();
        }
    }
}