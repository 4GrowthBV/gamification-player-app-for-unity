using System;
using GamificationPlayer.DTO.AnnounceDeviceFlow;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class PageViewDTO
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
            [OrganisationId]
            public string organisation_id;

            [UserId]
            public string user_id;
        }

        public Data data;
        
        public PageViewDTO()
        {
            data = new Data();
        }
    }
}
