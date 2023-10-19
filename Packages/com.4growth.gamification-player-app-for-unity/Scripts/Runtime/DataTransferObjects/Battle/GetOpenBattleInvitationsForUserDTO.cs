using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Session;
using UnityEngine;

namespace GamificationPlayer.DTO.Battle
{
    public class GetOpenBattleInvitationsForUserDTO
    {
        [Serializable]
        public class Attributes
        {
            
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            [BattleId]
            public string id;

            public string type = "battle_session";
        
            public Attributes attributes;

            public Data()
            {
                attributes = new Attributes();
            }            
        }

        public Data[] data;

        public GetOpenBattleInvitationsForUserDTO()
        {
            data = new Data[0];
        }
    }
}
