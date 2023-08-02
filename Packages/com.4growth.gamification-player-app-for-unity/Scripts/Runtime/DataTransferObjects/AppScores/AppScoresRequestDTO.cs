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

            public string ended_at;

            #nullable enable
            public string? module_session_id;
            public string? battle_session_id;
            public string? user_id;
            public string? organisation_id;
            public string? micro_game_id;
            public string? completed_at;
            #nullable disable
            
            public int score;

            public Attributes(DateTime endedAt, int score, DateTime? completedAt)
            {
                this.ended_at = endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                this.score = score;
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

            public Data(DateTime endedAt, int score, DateTime? completedAt)
            {
                attributes = new Attributes(endedAt, score, completedAt);
            }            
        }

        public Data data;

        public AppScoresRequestDTO(DateTime endedAt, 
            int score = 0, 
            DateTime? completedAt = null)
        {
            data = new Data(endedAt, score, completedAt);
        }
    }
}
