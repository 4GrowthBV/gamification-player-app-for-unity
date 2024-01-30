using System;
using System.Collections.Generic;

namespace GamificationPlayer.DTO.MicroGame
{
    public class GetMicroGameResponseDTO
    {
        [Serializable]
        public class Attributes
        {
            public string name;
            public string description;
            public string identifier;
            public int[] star_thresholds;
            public string web_gl_location;
            public Dictionary<string, string> extra_data;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }

            public float Time { get; set; }

            public string id;
            
            public string type;
            public Attributes attributes;

            public Data()
            {
                attributes = new Attributes();
            }
        }

        public Data data;

        public GetMicroGameResponseDTO()
        {
            data = new Data();
        }
    }
}
