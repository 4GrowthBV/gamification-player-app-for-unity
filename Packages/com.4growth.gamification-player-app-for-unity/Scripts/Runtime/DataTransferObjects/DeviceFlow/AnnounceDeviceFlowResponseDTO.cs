using System;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.AnnounceDeviceFlow
{
    public class AnnounceDeviceFlowResponseDTO
    {
        [Serializable]
        public class Attributes
        {
            public DateTime? ExpiredAt
            {
                get
                {
                    if(string.IsNullOrEmpty(expired_at))
                    {
                        return null;
                    }

                    return DateTime.Parse(expired_at); 
                }
            }

            public bool is_validated;

            #nullable enable
            public string? expired_at;
            #nullable disable

            [UserId]
            public string user_id;

            [OrganisationId]
            public string organisation_id;

            public string url;
        }

        [Serializable]
        public class Data : ILoggableData
        {
            public string Type { get => type; }
            public float Time { get; set; }
            
            [DeviceFlowId]
            public string id;

            public string type;
            
            public Attributes attributes;
        }
        
        public Data data;
    }
}