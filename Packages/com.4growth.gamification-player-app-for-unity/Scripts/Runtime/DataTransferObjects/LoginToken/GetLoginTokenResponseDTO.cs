using System;

namespace GamificationPlayer.DTO.LoginToken
{
    public class GetLoginTokenResponseDTO
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

            #nullable enable
            public string? expired_at;
            #nullable disable

            [Session.LoginToken]
            public string token;
        }

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

        public Data data;

        public GetLoginTokenResponseDTO()
        {
            data = new Data();
        }
    }
}