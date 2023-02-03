using System;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class MicroGamePayload : ILoggableData
    {
        [Serializable]
        public class Player
        {
            [OrganisationId]
            public string organisation_id;

            [UserId]
            public string user_id;
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

            public string name;

            public string identifier;

            public Stars stars;

            public MicroGame()
            {
                stars = new Stars();
            }
        }

        [Serializable]
        public class Module
        {
            public int mutiplier;

            public int max_score;

            public int current_score;

            public int current_bonus;

            public int current_total;
        }

        public Player player;
        public Session session;
        public MicroGame micro_game;
        public Module module;

        public string Type => "moduleData";

        public float Time { get; set; }

        public MicroGamePayload()
        {
            player = new Player();
            session = new Session();
            micro_game = new MicroGame();
            module = new Module();
        }
    }
}
