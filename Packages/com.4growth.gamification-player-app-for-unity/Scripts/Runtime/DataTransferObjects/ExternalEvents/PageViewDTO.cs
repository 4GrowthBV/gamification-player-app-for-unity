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

            [OrganisationAllowUpgradeToRegisteredUser]
            public bool organisation_allow_upgrade_to_registered_user;

            [UserId]
            public string user_id;

            [UserIsDemo]
            public bool user_is_demo;

            [UserAvatar]
            public bool user_avatar;

            [Language]
            public string language;

            [UserScore]
            public int user_score;

            [UserBonusScore]
            public int user_score_bonus;

            [UserBattleScore]
            public int user_score_battle;

            [OrganisationBattleActive]
            public bool organisation_battle_active;

            [TotalOpenBattleInvitationForUser]
            public int user_battle_invitations;
        }

        public Data data;
        
        public PageViewDTO()
        {
            data = new Data();
        }
    }
}
