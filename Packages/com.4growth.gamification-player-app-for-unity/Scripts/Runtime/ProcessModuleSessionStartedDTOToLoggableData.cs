using System;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.DTO.ModuleSession;

namespace GamificationPlayer
{
    public class ProcessModuleSessionStartedDTOToLoggableData
    {
        public ILoggableData Process(ModuleSessionStartedDTO dto)
        {
            var newDto = new UpdateModuleSessionResponseDTO();

            newDto.data.id = dto.data.attributes.module_session_id;
            newDto.data.attributes.started_at = DateTime.Now.ToString();
            newDto.data.attributes.ended_at = null;
            newDto.data.attributes.completed_at = null;
            newDto.data.relationships.challenge.data.id = dto.data.attributes.challenge_id;
            newDto.data.relationships.user.data.id = dto.data.attributes.user_id;

            newDto.data.type = "moduleSession";

            return newDto.data;
        }
    }
}