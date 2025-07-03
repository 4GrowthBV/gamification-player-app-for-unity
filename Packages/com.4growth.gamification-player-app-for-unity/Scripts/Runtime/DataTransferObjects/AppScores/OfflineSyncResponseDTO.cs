using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.OfflineSync
{
    [Serializable]
    public class OfflineSyncResponseDTO
    {
        [JsonProperty("data")]
        public List<ResponseItem> Data { get; set; }

        [Serializable]
        public class ResponseItem
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("attributes")]
            public ResponseAttributes Attributes { get; set; }

            [JsonProperty("meta")]
            public ResponseMeta Meta { get; set; }
        }

        [Serializable]
        public class ResponseAttributes
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
        }

        [Serializable]
        public class ResponseMeta
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("challenge_grouping")]
            public string ChallengeGrouping { get; set; }
        }
    }
}