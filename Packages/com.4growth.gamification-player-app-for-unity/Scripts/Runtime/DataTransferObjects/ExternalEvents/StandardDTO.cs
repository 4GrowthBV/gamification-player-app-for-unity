using System;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class StandardDTO
    {
        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            public string type;
        }

        public Data data;

        public StandardDTO()
        {
            data = new Data();
        }
    }
}
