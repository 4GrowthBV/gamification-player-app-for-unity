using System;
using UnityEngine;

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

            public string module_session_id;
            public string battle_session_id;
            public string user_id;
            public string organisation_id;
            public string micro_game_id;

            public string ended_at;

            #nullable enable
            public string? completed_at;
            #nullable disable
            
            public int score;

            public Attributes(DateTime endedAt, 
                int score, 
                Guid moduleSessionId,
                Guid battleSessionId,
                Guid userId,
                Guid organisationId,
                string microGameId,
                DateTime? completedAt)
            {
                this.ended_at = endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");

                this.score = score;

                this.module_session_id = moduleSessionId == Guid.Empty ? string.Empty : moduleSessionId.ToString();
                this.battle_session_id = battleSessionId == Guid.Empty ? string.Empty : battleSessionId.ToString();
                this.user_id = userId == Guid.Empty ? string.Empty : userId.ToString();
                this.organisation_id = organisationId == Guid.Empty ? string.Empty : organisationId.ToString();
                this.micro_game_id = microGameId.ToString();                
                this.completed_at = ended_at == null ? string.Empty : completedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ");

                Valid();
            }

            public void Valid()
            {
                if(!string.IsNullOrEmpty(module_session_id) && !string.IsNullOrEmpty(battle_session_id))
                {
                    throw new Exception("module_session_id and battle_session_id cannot be set at the same time.");
                }

                if(!string.IsNullOrEmpty(user_id) && !string.IsNullOrEmpty(module_session_id))
                {
                    throw new Exception("user_id and module_session_id cannot be set at the same time.");
                }

                if(!string.IsNullOrEmpty(organisation_id) && !string.IsNullOrEmpty(module_session_id))
                {
                    throw new Exception("organisation_id and module_session_id cannot be set at the same time.");
                }

                if(!string.IsNullOrEmpty(micro_game_id) && (!string.IsNullOrEmpty(module_session_id) || !string.IsNullOrEmpty(battle_session_id)))
                {
                    throw new Exception("micro_game_id cannot be set at the same time as module_session_id or battle_session_id.");
                }
            }
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }
            public string type = "app_score";
        
            public Attributes attributes;

            public Data(DateTime endedAt, 
                int score, 
                Guid moduleSessionId,
                Guid battleSessionId,
                Guid userId,
                Guid organisationId,
                string microGameId,
                DateTime? completedAt)
            {
                attributes = new Attributes(endedAt, score, moduleSessionId, battleSessionId, userId, organisationId, microGameId, completedAt);
            }            
        }

        public Data data;

        public AppScoresRequestDTO(DateTime endedAt, 
            int score, 
            Guid moduleSessionId,
            Guid battleSessionId,
            Guid userId,
            Guid organisationId,
            string microGameId,
            DateTime? completedAt)
        {
            data = new Data(endedAt, score, moduleSessionId, battleSessionId, userId, organisationId, microGameId, completedAt);
        }
    }
}
