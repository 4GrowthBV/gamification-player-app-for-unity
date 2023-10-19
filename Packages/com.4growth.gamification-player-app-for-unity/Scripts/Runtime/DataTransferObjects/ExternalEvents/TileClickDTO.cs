using System;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class TileClickDTO
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
            public string identifier;
        }

        public Data data;
        
        public TileClickDTO()
        {
            data = new Data();
        }
    }
}
