using System;
using GamificationPlayer.Session;

namespace GamificationPlayer
{
    public class GetUserStatisticsDTO
    {
        [Serializable]
        public class Attributes
        {
            [UserScore]
            public int score;

            [UserBonusScore]
            public int bonus_score;

            [UserBattleScore]
            public int battle_score;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            [UserId]
            public string id;
            
            public string type;
            public Attributes attributes;

            public Data()
            {
                attributes = new Attributes();
            }
        }

        public Data data;

        public GetUserStatisticsDTO()
        {
            data = new Data();
        }
    }
}
