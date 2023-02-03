using System;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class MicroGameOpenedDTO
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
            [MicroGameIdentifier]
            public string identifier;

            [MicroGameJSONWebToken]
            public string module_data;
        }

        public Data data;
        
        public MicroGameOpenedDTO()
        {
            data = new Data();
        }
    }
}
