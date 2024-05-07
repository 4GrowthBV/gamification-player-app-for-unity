using System;
using System.Linq;
using GamificationPlayer.Session;

namespace GamificationPlayer
{
    public class UserTagsDataHelper : ILoggableData
    {
        public string Type { get; } = "user_tags";

        public float Time { get; set; }

        [UserTags]
        public string[] Tags;

        public UserTagsDataHelper(GetUserResponseDTO getUserResponseDTO)
        {
            if(getUserResponseDTO == null)
                return;

            if(getUserResponseDTO.included == null)
                return;

            Tags = getUserResponseDTO.included.Select(x => x.attributes.name).ToArray();
        }
    }

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
        public class Tags
        {
            [Serializable]
            public class Attributes
            {
                public string name;
            }

            public string id;
            
            public string type;
            public Attributes attributes;
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

        public Tags[] included;

        public GetUserResponseDTO()
        {
            data = new Data();
        }
    }
}
