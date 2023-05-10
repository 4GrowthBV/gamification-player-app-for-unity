using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class ErrorDTO
    {
        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            public string type;

            public Attributes attributes;

            public Data()
            {
                attributes = new Attributes();
            }
        }

        [Serializable]
        public class Attributes
        {  
            public string title;

            public string code;
        }

        public Data data;
        
        public ErrorDTO()
        {
            data = new Data();
        }
    }
}
