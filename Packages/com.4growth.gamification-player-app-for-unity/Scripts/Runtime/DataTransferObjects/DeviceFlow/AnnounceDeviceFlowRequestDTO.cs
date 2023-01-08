using System;

namespace GamificationPlayer.DTO.AnnounceDeviceFlow
{
    public class AnnounceDeviceFlowRequestDTO
    {
        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }

            public string type = "device_login";            
        }

        public Data data;

        public AnnounceDeviceFlowRequestDTO()
        {
            data = new Data();
        }
    }
}