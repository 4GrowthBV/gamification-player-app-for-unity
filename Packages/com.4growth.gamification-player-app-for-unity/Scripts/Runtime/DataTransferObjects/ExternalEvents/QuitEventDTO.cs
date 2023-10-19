using System;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class QuitEventDTO
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
        }

        public Data data;
        
        public QuitEventDTO()
        {
            data = new Data();
        }
    }
}
