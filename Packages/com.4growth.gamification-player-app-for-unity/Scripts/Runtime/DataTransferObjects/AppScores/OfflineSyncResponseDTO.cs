using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.OfflineSync
{
    [Serializable]
    public class OfflineSyncResponseDTO
    {
        [JsonProperty("data")]
        public List<ResponseItem> Data;

        [Serializable]
        public class ResponseItem
        {
            [JsonProperty("type")]
            public string Type;

            [JsonProperty("attributes")]
            public ResponseAttributes Attributes;

            [JsonProperty("meta")]
            public ResponseMeta Meta;
        }

        [Serializable]
        public class ResponseAttributes
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
        }

        [Serializable]
        public class ResponseMeta
        {
            [JsonProperty("status")]
            public string Status;

            [JsonProperty("challenge_grouping")]
            public string ChallengeGrouping;
        }
    }
}