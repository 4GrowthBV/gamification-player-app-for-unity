using System;
using GamificationPlayer.Session;

namespace GamificationPlayer
{
    public class GetUserResponseDTO
    {
        [Serializable]
        public class Attributes
        {
            [UserName]
            public string name;

            [UserEmail]
            public string email;

            [UserAvatar]
            public string avatar;
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

        public GetUserResponseDTO()
        {
            data = new Data();
        }
    }
}
