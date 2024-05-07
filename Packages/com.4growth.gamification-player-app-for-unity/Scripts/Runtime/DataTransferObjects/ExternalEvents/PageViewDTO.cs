using System;
using GamificationPlayer.DTO.AnnounceDeviceFlow;
using GamificationPlayer.Session;
using Newtonsoft.Json;

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

            [OrganisationAllowUpgradeToRegisteredUser]
            public bool organisation_allow_upgrade_to_registered_user;

            [UserId]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string user_id;

            [UserIsDemo]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool user_is_demo;

            [UserAvatar]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string user_avatar;

            [Language]
            public string language;

            [UserScore]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int user_score;

            [UserBonusScore]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int user_score_bonus;

            [UserBattleScore]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int user_score_battle;

            [OrganisationBattleActive]
            public bool organisation_battle_active;

            [TotalOpenBattleInvitationForUser]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int user_battle_invitations;
       
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            [UserTags]
            public string[] user_tags;
        }

        public Data data;
        
        public PageViewDTO()
        {
            data = new Data();
        }
    }
}
