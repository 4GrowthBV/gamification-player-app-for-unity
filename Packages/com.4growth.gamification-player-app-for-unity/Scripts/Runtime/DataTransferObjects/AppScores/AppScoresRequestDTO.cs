using System;
using System.Collections.Generic;
using System.Globalization;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.Session;
using Newtonsoft.Json;
using UnityEngine;

namespace GamificationPlayer.DTO.AppScores
{
    /// <summary>
    /// Represents the top‐level request body for storing an app score.
    /// </summary>
    public class AppScoresRequestDTO
    {
        [JsonProperty("data")]
        public DataContainer Data { get; }

        private AppScoresRequestDTO(DataContainer data)
        {
            Data = data;
        }

        /// <summary>
        /// Inner class matching the schema’s "data" object.
        /// </summary>
        public class DataContainer : ILoggableData
        {
            // Always "app_score"
            [JsonProperty("type")]
            public string Type => "app_score";

            [JsonProperty("attributes")]
            public AttributesContainer Attributes { get; }
            public float Time { get; set; }

            public DataContainer(AttributesContainer attributes)
            {
                Attributes = attributes;
            }
        }

        /// <summary>
        /// Inner class matching the schema’s "attributes" object and validation rules.
        /// </summary>
        public class AttributesContainer
        {
            // --------------------
            // Schema fields:
            // --------------------

            [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
            public string UserId { get; }

            [JsonProperty("organisation_id", NullValueHandling = NullValueHandling.Ignore)]
            public string OrganisationId { get; }

            [JsonProperty("micro_game_id", NullValueHandling = NullValueHandling.Ignore)]
            public string MicroGameId { get; }

            [JsonProperty("started_at", NullValueHandling = NullValueHandling.Ignore)]
            public string StartedAt { get; }

            [JsonProperty("ended_at")]
            public string EndedAt { get; }

            [JsonProperty("completed_at", NullValueHandling = NullValueHandling.Ignore)]
            public string CompletedAt { get; }

            [JsonProperty("score")]
            public int Score { get; }

            [JsonProperty("integration", NullValueHandling = NullValueHandling.Ignore)]
            public MicroGamePayload.Integration Integration { get; }

            private string contextType;

            // --------------------
            // Constructor & validation
            // --------------------
            private AttributesContainer(
                string contextType,
                Guid? userId,
                Guid? organisationId,
                Guid? microGameId,
                DateTime? startedAt,
                DateTime endedAt,
                DateTime? completedAt,
                int score,
                MicroGamePayload.Integration integration)
            {
                this.contextType = contextType;
                UserId = userId?.ToString();
                OrganisationId = organisationId?.ToString();
                MicroGameId = microGameId?.ToString();
                EndedAt = FormatDateTime(endedAt);
                Score = score;

                if (startedAt.HasValue)
                {
                    StartedAt = FormatDateTime(startedAt.Value);
                }

                if (completedAt.HasValue)
                {
                    CompletedAt = FormatDateTime(completedAt.Value);
                }

                Integration = integration;

                ValidateProhibitions();
            }

            private static string FormatDateTime(DateTime dt)
            {
                // Convert to UTC "yyyy-MM-ddTHH:mm:ssZ"
                return dt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
            }

            private void ValidateProhibitions()
            {
                // 1) If context_type is 'module_session', then:
                //    - user_id is prohibited
                //    - organisation_id is prohibited
                //    - micro_game_id is prohibited
                //    - started_at is prohibited
                if (contextType == "module_session")
                {
                    if (!string.IsNullOrEmpty(UserId))
                        throw new Exception("user_id is prohibited when context_type = module_session.");
                    if (!string.IsNullOrEmpty(OrganisationId))
                        throw new Exception("organisation_id is prohibited when context_type = module_session.");
                    if (!string.IsNullOrEmpty(MicroGameId))
                        throw new Exception("micro_game_id is prohibited when context_type = module_session.");
                    if (!string.IsNullOrEmpty(StartedAt))
                        throw new Exception("started_at is prohibited when context_type = module_session.");
                }

                // 2) If context_type is any of [module_session, battle_session, direct_play, daily_challenge],
                //    then organisation_id and micro_game_id are prohibited.
                var prohibitedForMany = new HashSet<string>
                    { "module_session", "battle_session", "direct_play", "daily_challenge" };
                if (!string.IsNullOrEmpty(contextType) && prohibitedForMany.Contains(contextType))
                {
                    if (!string.IsNullOrEmpty(OrganisationId))
                        throw new Exception($"organisation_id is prohibited when context_type = {contextType}.");
                    if (!string.IsNullOrEmpty(MicroGameId))
                        throw new Exception($"micro_game_id is prohibited when context_type = {contextType}.");
                }

                // 4) Score must be non-negative (schema says integer, but typically we validate business logic)
                if (Score < 0)
                {
                    throw new Exception("score must be a non-negative integer.");
                }
            }

            // --------------------
            // Static factory methods
            // --------------------

            /// <summary>
            /// For a module session context: context_type = "module_session", context_id = moduleSessionId.
            /// All other context‐prohibited fields will be omitted automatically.
            /// </summary>
            public static AttributesContainer ForModuleSession(
                DateTime endedAt,
                int score,
                DateTime? completedAt = null,
                MicroGamePayload.Integration integration = null)
            {
                return new AttributesContainer(
                    contextType: "module_session",
                    userId: null,
                    organisationId: null,
                    microGameId: null,
                    startedAt: null,            // prohibited in module_session
                    endedAt: endedAt,
                    completedAt: completedAt,
                    score: score,
                    integration: integration
                );
            }

            /// <summary>
            /// For a battle session context: context_type = "battle_session", context_id = battleSessionId.
            /// user_id is the player. ended_at, completed_at, score still apply.
            /// </summary>
            public static AttributesContainer ForBattleSession(
                Guid userId,
                DateTime startedAt,
                DateTime endedAt,
                int score,
                DateTime? completedAt = null,
                MicroGamePayload.Integration integration = null)
            {
                return new AttributesContainer(
                    contextType: "battle_session",
                    userId: userId,
                    organisationId: null,
                    microGameId: null,
                    startedAt: startedAt,
                    endedAt: endedAt,
                    completedAt: completedAt,
                    score: score,
                    integration: integration
                );
            }

            /// <summary>
            /// For a direct play context: context_type = "direct_play", context_id = directPlaySessionId.
            /// user_id is the player. ended_at, completed_at, score still apply.
            /// </summary>
            public static AttributesContainer ForDirectPlay(
                Guid userId,
                DateTime startedAt,
                DateTime endedAt,
                int score,
                DateTime? completedAt = null,
                MicroGamePayload.Integration integration = null)
            {
                return new AttributesContainer(
                    contextType: "direct_play",
                    userId: userId,
                    organisationId: null,
                    microGameId: null,
                    startedAt: startedAt,
                    endedAt: endedAt,
                    completedAt: completedAt,
                    score: score,
                    integration: integration
                );
            }

            /// <summary>
            /// For a daily challenge: context_type = "daily_challenge", context_id = dailyChallengeId.
            /// user_id is the player. micro_game_id is the challenged game. ended_at, completed_at, score still apply.
            /// </summary>
            public static AttributesContainer ForDailyChallenge(
                Guid userId,
                DateTime startedAt,
                DateTime endedAt,
                int score,
                DateTime? completedAt = null,
                MicroGamePayload.Integration integration = null)
            {
                return new AttributesContainer(
                    contextType: "daily_challenge",
                    userId: userId,
                    organisationId: null,    // prohibited for daily_challenge
                    microGameId: null,
                    startedAt: startedAt,
                    endedAt: endedAt,
                    completedAt: completedAt,
                    score: score,
                    integration: integration
                );
            }

            /// <summary>
            /// No context (context_type = "none"). Must supply user_id, organisation_id, micro_game_id.
            /// </summary>
            public static AttributesContainer ForNoContext(
                Guid userId,
                Guid organisationId,
                Guid microGameId,
                DateTime startedAt,
                DateTime endedAt,
                int score,
                DateTime? completedAt = null,
                MicroGamePayload.Integration integration = null)
            {
                return new AttributesContainer(
                    contextType: "none",
                    userId: userId,
                    organisationId: organisationId,
                    microGameId: microGameId,
                    startedAt: startedAt,
                    endedAt: endedAt,
                    completedAt: completedAt,
                    score: score,
                    integration: integration
                );
            }
        }

        // --------------------
        // Public static constructors for the top‐level request
        // --------------------

        /// <summary>
        /// Creates a request for a module session.
        /// </summary>
        public static AppScoresRequestDTO CreateModuleSessionRequest(
            DateTime endedAt,
            int score,
            DateTime? completedAt = null,
            MicroGamePayload.Integration integration = null)
        {
            var attrs = AttributesContainer.ForModuleSession(
                endedAt,
                score,
                completedAt,
                integration
            );
            return new AppScoresRequestDTO(new DataContainer(attrs));
        }

        /// <summary>
        /// Creates a request for a battle session.
        /// </summary>
        public static AppScoresRequestDTO CreateBattleSessionRequest(
            Guid userId,
            DateTime startedAt,
            DateTime endedAt,
            int score,
            DateTime? completedAt = null,
            MicroGamePayload.Integration integration = null)
        {
            var attrs = AttributesContainer.ForBattleSession(
                userId,
                startedAt,
                endedAt,
                score,
                completedAt,
                integration
            );
            return new AppScoresRequestDTO(new DataContainer(attrs));
        }

        /// <summary>
        /// Creates a request for a direct play.
        /// </summary>
        public static AppScoresRequestDTO CreateDirectPlayRequest(
            Guid userId,
            DateTime startedAt,
            DateTime endedAt,
            int score,
            DateTime? completedAt = null,
            MicroGamePayload.Integration integration = null)
        {
            var attrs = AttributesContainer.ForDirectPlay(
                userId,
                startedAt,
                endedAt,
                score,
                completedAt,
                integration
            );
            return new AppScoresRequestDTO(new DataContainer(attrs));
        }

        /// <summary>
        /// Creates a request for a daily challenge.
        /// </summary>
        public static AppScoresRequestDTO CreateDailyChallengeRequest(
            Guid userId,
            DateTime startedAt,
            DateTime endedAt,
            int score,
            DateTime? completedAt = null,
            MicroGamePayload.Integration integration = null)
        {
            var attrs = AttributesContainer.ForDailyChallenge(
                userId,
                startedAt,
                endedAt,
                score,
                completedAt,
                integration
            );
            return new AppScoresRequestDTO(new DataContainer(attrs));
        }

        /// <summary>
        /// Creates a request with no context (type = "none").
        /// </summary>
        public static AppScoresRequestDTO CreateNoContextRequest(
            Guid userId,
            Guid organisationId,
            Guid microGameId,
            DateTime startedAt,
            DateTime endedAt,
            int score,
            DateTime? completedAt = null,
            MicroGamePayload.Integration integration = null)
        {
            var attrs = AttributesContainer.ForNoContext(
                userId,
                organisationId,
                microGameId,
                startedAt,
                endedAt,
                score,
                completedAt,
                integration
            );
            return new AppScoresRequestDTO(new DataContainer(attrs));
        }
    }
}
