using System;
using GamificationPlayer.DTO.AnnounceDeviceFlow;
using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.ExternalEvents
{
    public class ModuleSessionStartedDTO
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
            
            [CampaignId]
            public string campaign_id;

            [ChallengeId]
            public string challenge_id;

            [ChallengeSessionId]
            public string challenge_session_id;

            [ModuleId]
            public string module_id;

            [ModuleSessionId]
            public string module_session_id;
        }

        public Data data;

        public ModuleSessionStartedDTO()
        {
            data = new Data();
        }
    }
}
