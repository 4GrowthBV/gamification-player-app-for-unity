using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Session;
using UnityEngine;

namespace GamificationPlayer.DTO.Battle
{
    public class ActiveBattleDTO
    {
        [Serializable]
        public class Attributes
        {
            [BattleName]
            public string name;

            //geen idee wat dit is?
            public string game_id;

            [BattleMicroGameId]
            public string micro_game_id;

            [BattleAvailableFrom]
            public string available_from;

            [BattleAvailableTill]
            public string available_till;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            [BattleId]
            public string id;

            public string type = "battle";
        
            public Attributes attributes;

            public Data()
            {
                attributes = new Attributes();
            }            
        }

        public Data data;

        public ActiveBattleDTO()
        {
            data = new Data();
        }
    }
}
