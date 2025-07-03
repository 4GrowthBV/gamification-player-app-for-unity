using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.OfflineSync
{
    [Serializable]
    public class OfflineSyncRequestDTO
    {
        [JsonProperty("meta")]
        public MetaData Meta { get; set; }

        [JsonProperty("data")]
        public List<DataItem> Data { get; set; }

        [Serializable]
        public class MetaData
        {
            [JsonProperty("organisation_id")]
            public string OrganisationId { get; set; }

            [JsonProperty("user_id")]
            public string UserId { get; set; }
        }

        [Serializable]
        public class DataItem
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("attributes")]
            public Attributes Attributes { get; set; }

            [JsonProperty("meta")]
            public ItemMeta Meta { get; set; }
        }

        [Serializable]
        public class Attributes
        {
            [JsonProperty("module_id")]
            public string ModuleId { get; set; }

            [JsonProperty("started_at")]
            public string StartedAt { get; set; }

            [JsonProperty("ended_at")]
            public string EndedAt { get; set; }

            [JsonProperty("completed_at")]
            public string CompletedAt { get; set; }

            [JsonProperty("score")]
            public int Score { get; set; }

            [JsonProperty("extra_data")]
            public Dictionary<string, object> ExtraData { get; set; }
        }

        [Serializable]
        public class ItemMeta
        {
            [JsonProperty("challenge_grouping")]
            public string ChallengeGrouping { get; set; }
        }
    }
}