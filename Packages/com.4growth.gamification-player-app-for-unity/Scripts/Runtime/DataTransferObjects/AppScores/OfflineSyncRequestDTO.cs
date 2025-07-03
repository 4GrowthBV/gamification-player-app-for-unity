using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.OfflineSync
{
    [Serializable]
    public class OfflineSyncRequestDTO
    {
        [JsonProperty("meta")]
        public MetaData Meta;

        [JsonProperty("data")]
        public List<DataItem> Data;

        [Serializable]
        public class MetaData
        {
            [JsonProperty("organisation_id")]
            public string OrganisationId;

            [JsonProperty("user_id")]
            public string UserId;
        }

        [Serializable]
        public class DataItem
        {
            [JsonProperty("type")]
            public string Type;

            [JsonProperty("attributes")]
            public Attributes Attributes;

            [JsonProperty("meta")]
            public ItemMeta Meta;
        }

        [Serializable]
        public class Attributes
        {
            [JsonProperty("module_id")]
            public string ModuleId;

            [JsonProperty("started_at")]
            public string StartedAt;

            [JsonProperty("ended_at")]
            public string EndedAt;

            [JsonProperty("completed_at")]
            public string CompletedAt;

            [JsonProperty("score")]
            public int Score;

            [JsonProperty("extra_data")]
            public Dictionary<string, object> ExtraData;
        }

        [Serializable]
        public class ItemMeta
        {
            [JsonProperty("challenge_grouping")]
            public string ChallengeGrouping;
        }
    }
}