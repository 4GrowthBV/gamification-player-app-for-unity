using System;

namespace GamificationPlayer.DTO.AppScores
{
    public class AppScoresRequestDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime EndedAt
            {
                get
                {
                    return DateTime.Parse(ended_at.Remove(ended_at.Length - 1, 1));
                }
            }

            public DateTime StartedAt
            {
                get
                {
                    return DateTime.Parse(started_at.Remove(started_at.Length - 1, 1));
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

                    return DateTime.Parse(completed_at.Remove(completed_at.Length - 1, 1));
                }
            }

            
            public string battle_session_id;
            public string user_id;
            public string organisation_id;

            public string ended_at;
            public string started_at;

            #nullable enable
            public string? completed_at;
            #nullable disable
            
            public int score;

            public Attributes(DateTime startedAt,
                DateTime endedAt, 
                int score, 
                Guid userId,
                Guid organisationId,
                Guid battleSessionId,
                DateTime? completedAt)
            {
                this.started_at = startedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                this.ended_at = endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                this.score = score;
                this.user_id = userId.ToString();
                this.organisation_id = organisationId.ToString();
                this.battle_session_id = battleSessionId.ToString();
                this.completed_at = completedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }
            public string type = "app_score";
        
            public Attributes attributes;

            public Data(DateTime startedAt,
                DateTime endedAt, 
                int score, 
                Guid userId,
                Guid organiastionId,
                Guid battleSessionId,
                DateTime? completedAt)
            {
                attributes = new Attributes(startedAt, endedAt, score, userId, organiastionId, battleSessionId, completedAt);
            }            
        }

        public Data data;

        public AppScoresRequestDTO(DateTime startedAt,
            DateTime endedAt, 
            Guid userId,
            Guid organisationId,
            Guid battleSessionId,
            int score = 0, 
            DateTime? completedAt = null)
        {
            data = new Data(startedAt, endedAt, score, userId, organisationId, battleSessionId, completedAt);
        }
    }
}
