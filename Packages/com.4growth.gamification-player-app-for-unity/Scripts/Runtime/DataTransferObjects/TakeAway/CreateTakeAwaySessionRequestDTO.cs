using System;
using System.Globalization;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.TakeAway
{
    public class CreateTakeAwaySessionRequestDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime StartedAt
            {
                get
                {
                    var date = started_at.Replace("Z", "");

                    return DateTime.ParseExact(date, 
                        "yyyy-MM-ddTHH:mm:ss", 
                        CultureInfo.InvariantCulture);
                }
            }

            public DateTime EndedAt
            {
                get
                {
                    var date = ended_at.Replace("Z", "");

                    return DateTime.ParseExact(date, 
                        "yyyy-MM-ddTHH:mm:ss", 
                        CultureInfo.InvariantCulture);
                }
            }

            public string started_at;

            public string ended_at;
            
            public Attributes(DateTime startedAt,
                DateTime endedAt)
            {
                this.started_at = startedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                this.ended_at = endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }
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

                    public Data(string type, Guid id)
                    {
                        this.type = type;
                        this.id = id.ToString();
                    }
                }

                public Data data;

                public Type(string type, Guid id)
                {
                    data = new Data(type, id);
                }
            }

            public Type micro_game;

            public Type user;

            public Type organisation;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Type module_session = null;

            public Relationships(Guid microGameId,
                Guid userId,
                Guid organisationId,
                Guid? sessionId)
            {
                micro_game = new Type("micro_game", microGameId);
                user = new Type("user", userId);
                organisation = new Type("organisation", organisationId);

                if (sessionId != null)
                {
                    module_session = new Type("module_session", sessionId.Value);
                }
            }
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            public string type = "take_away_session";

            public Attributes attributes;
            public Relationships relationships;

            public Data(DateTime startedAt,
                DateTime endedAt,
                Guid microGameId,
                Guid userId,
                Guid organisationId,
                Guid? sessionId)
            {
                attributes = new Attributes(startedAt, endedAt);
                relationships = new Relationships(microGameId, userId, organisationId, sessionId);
            }
        }

        public Data data;

        public CreateTakeAwaySessionRequestDTO(DateTime startedAt,
            DateTime endedAt,
            Guid microGameId,
            Guid userId,
            Guid organisationId,
            Guid? sessionId)
        {
            data = new Data(startedAt, endedAt, microGameId, userId, organisationId, sessionId);
        }
    }
}
