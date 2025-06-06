using System;
using System.Collections.Generic;
using GamificationPlayer.Session;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class MicroGamePayload : ILoggableData
    {
        [Serializable]
        public class Environment
        {
            [EnvironmentDomain]
            public string domain;

            [EnvironmentType]
            public string type;
        }

        [Serializable]
        public class Player
        {
            [OrganisationId]
            public string organisation_id;

            [UserId]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string user_id;

            [UserAvatar]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string user_avatar;

            [UserName]
            public string user_name;

            [Language]
            public string language;

            [UserTags]
            public string[] user_tags;

            [UserRole]
            public string role;
        }

        [Serializable]
        public class Session
        {
            [ChallengeSessionId]
            public string challenge_session_id;

            [ModuleSessionId]
            public string module_session_id;
        }

        [Serializable]
        public class Battle
        {
            public string battle_id;

            [BattleSessionId]
            public string battle_session_id;

            [BattleOpponentName]
            public string opponent_name;
            
            [BattleOpponentAvatar]
            public string opponent_avatar;
        }

        [Serializable]
        public class MicroGame
        {
            [Serializable]
            public class Stars
            {
                public int five;

                public int four;

                public int three;

                public int two;

                public int one;
            }

            [MicroGameId]
            public string id;

            public string name;

            [MicroGameIdentifier]
            public string identifier;

            public Stars stars;

            public Dictionary<string, string> extra_data;

            public MicroGame()
            {
                stars = new Stars();
            }
        }

        [Serializable]
        public class Module
        {
            [ModuleId]
            public string id;

            public int multiplier;

            public int max_score;

            public int current_score;

            public int current_bonus;

            public int current_total;
        }    

        [Serializable]
        public class Integration
        {
            public string id;

            public Dictionary<string, string> context;
        }    

        public Player player;
        public Session session;
        public Battle battle;
        public MicroGame micro_game;
        public Module module;
        public Environment environment;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Integration integration;

        [ContextType]
        public string context_type;

        [SubmitToken]
        public string submit_token;

        public string Type => "moduleData";

        public float Time { get; set; }

        public MicroGamePayload()
        {
            player = new Player();
            session = new Session();
            battle = new Battle();
            micro_game = new MicroGame();
            module = new Module();
            environment = new Environment();
            integration = new Integration();
        }
    }
}
