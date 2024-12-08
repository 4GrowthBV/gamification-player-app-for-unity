using System;
using Newtonsoft.Json;

namespace GamificationPlayer.DTO.TakeAway
{
    public class GetTakeAwaySessionsRequestDTO 
    {
        public string micro_game_id;
        public string user_id;
        public string organisation_id;

        #nullable enable
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? module_session_id;
        #nullable disable

        public GetTakeAwaySessionsRequestDTO(Guid microGameId, 
            Guid userId, 
            Guid organisationId, 
            Guid? moduleSessionId = null)
        {
            micro_game_id = microGameId.ToString();
            user_id = userId.ToString();
            organisation_id = organisationId.ToString();

            if(moduleSessionId != null)
            {
                module_session_id = moduleSessionId.Value.ToString();
            }
        }
    }
}
