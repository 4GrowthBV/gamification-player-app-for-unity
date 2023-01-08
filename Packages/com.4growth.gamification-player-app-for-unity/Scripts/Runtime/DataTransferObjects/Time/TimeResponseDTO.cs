using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Session;
using UnityEngine;

namespace GamificationPlayer
{
    public class TimeResponseDTO
    {
        [Serializable]
        public class Attributes
        {   
            public DateTime Now
            {
                get
                {
                    return DateTime.Parse(now);
                }
            }

            [TimeNow]
            public string now;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get => time; set => time = value; }
            
            [TimeNowLogged]
            public float time;

            public string type;

            public Attributes attributes;

            public Data()
            {
                attributes = new Attributes();
            } 
        }
        
        public Data data;

        public TimeResponseDTO()
        {
            data = new Data();
        }
    }
}
