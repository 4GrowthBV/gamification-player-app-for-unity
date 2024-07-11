using System;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.Session;
using Newtonsoft.Json;
using UnityEngine;

namespace GamificationPlayer.DTO.AppScores
{
    public class AppScoresRequestDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime? StartedAt
            {
                get
                {
                    if(string.IsNullOrEmpty(started_at))
                    {
                        return null;
                    }

                    return DateTime.Parse(started_at.Remove(started_at.Length - 1, 1));
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

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string module_session_id;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string battle_session_id;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string user_id;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string organisation_id;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string micro_game_id;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string started_at;

            public string ended_at;

            [MicroGameCompletedAt]
            public string completed_at;
            
            [MicroGameScore]
            public int score;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public MicroGamePayload.Integration integration;

            public Attributes(DateTime startedAt,
                DateTime endedAt, 
                int score, 
                Guid moduleSessionId,
                Guid battleSessionId,
                Guid userId,
                Guid organisationId,
                Guid microGameId,
                DateTime? completedAt,
                MicroGamePayload.Integration integration = null)
            {
                this.score = score;

                this.module_session_id = moduleSessionId == Guid.Empty ? null : moduleSessionId.ToString();
                this.battle_session_id = battleSessionId == Guid.Empty ? null : battleSessionId.ToString();
                this.user_id = userId == Guid.Empty ? null : userId.ToString();
                this.organisation_id = organisationId == Guid.Empty ? null : organisationId.ToString();
                this.micro_game_id = microGameId == Guid.Empty ? null : microGameId.ToString();     
                this.integration = IsValid(integration) ? integration : null;

                //Have to double check with Dick, I thought that started_at was always needed
                //But the API docs says it is not needed in a module session
                if(string.IsNullOrEmpty(module_session_id))
                {
                    this.started_at = startedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }

                this.ended_at = endedAt == null ? null : endedAt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                this.completed_at = completedAt == null ? null : completedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ");

                Valid();
            }

            private bool IsValid(MicroGamePayload.Integration integration)
            {
                if(integration == null)
                {
                    return false;
                }

                if(string.IsNullOrEmpty(integration.id))
                {
                    return false;
                }

                return true;
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
                
                if(!string.IsNullOrEmpty(module_session_id) && !string.IsNullOrEmpty(started_at))
                {
                    throw new Exception("module_session_id and started_at cannot be set at the same time.");
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

            public Data(DateTime startedAt,
                DateTime endedAt, 
                int score, 
                Guid moduleSessionId,
                Guid battleSessionId,
                Guid userId,
                Guid organisationId,
                Guid microGameId,
                DateTime? completedAt,
                MicroGamePayload.Integration integration = null)
            {
                attributes = new Attributes(startedAt, 
                    endedAt, 
                    score, 
                    moduleSessionId, 
                    battleSessionId, 
                    userId, 
                    organisationId, 
                    microGameId, 
                    completedAt, 
                    integration);
            }            
        }

        public Data data;

        public AppScoresRequestDTO(DateTime startedAt, 
            DateTime endedAt, 
            int score, 
            Guid moduleSessionId,
            Guid battleSessionId,
            Guid userId,
            Guid organisationId,
            Guid microGameId,
            DateTime? completedAt,
            MicroGamePayload.Integration integration = null)
        {
            data = new Data(startedAt, 
                endedAt, 
                score, 
                moduleSessionId, 
                battleSessionId, 
                userId, 
                organisationId, 
                microGameId, 
                completedAt, 
                integration);
        }

        public static AppScoresRequestDTO GetAppScoresBattleRequest(DateTime startedAt,
            DateTime endedAt, 
            int score, 
            Guid userId,
            Guid battelSessionId,
            DateTime? completedAt,
            MicroGamePayload.Integration integration = null)
        {
            return new AppScoresRequestDTO(startedAt, 
                endedAt,
                score, 
                Guid.Empty, 
                battelSessionId, 
                userId, 
                Guid.Empty, 
                Guid.Empty, 
                completedAt, 
                integration);
        }

        public static AppScoresRequestDTO GetAppScoresModuleRequest(DateTime startedAt,
            DateTime endedAt, 
            int score, 
            Guid moduleSessionId,
            DateTime? completedAt,
            MicroGamePayload.Integration integration = null)
        {
            return new AppScoresRequestDTO(startedAt, 
                endedAt, 
                score, 
                moduleSessionId, 
                Guid.Empty, 
                Guid.Empty, 
                Guid.Empty, 
                Guid.Empty, 
                completedAt, 
                integration);
        }

        public static AppScoresRequestDTO GetAppScoresRequest(DateTime startedAt,
            DateTime endedAt, 
            int score, 
            Guid userId,
            Guid organisationId,
            Guid microGameId,
            DateTime? completedAt,
            MicroGamePayload.Integration integration = null)
        {
            return new AppScoresRequestDTO(startedAt, 
                endedAt, 
                score, 
                Guid.Empty, 
                Guid.Empty,
                userId, 
                organisationId, 
                microGameId, 
                completedAt, 
                integration);
        }
    }
}
