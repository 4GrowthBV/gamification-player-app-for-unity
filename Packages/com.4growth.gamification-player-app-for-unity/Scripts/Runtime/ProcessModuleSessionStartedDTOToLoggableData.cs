using System;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.DTO.ModuleSession;

namespace GamificationPlayer
{
    public class ProcessModuleSessionStartedDTOToLoggableData
    {
        public ILoggableData Process(ModuleSessionStartedDTO.Data data, Guid userId)
        {
            var newDto = new UpdateModuleSessionResponseDTO();

            newDto.data.id = data.attributes.module_session_id;
            newDto.data.attributes.started_at = DateTime.Now.ToString();
            newDto.data.attributes.ended_at = null;
            newDto.data.attributes.completed_at = null;
            newDto.data.relationships.challenge_session.data.id = data.attributes.challenge_id;
            newDto.data.relationships.module.data.id = userId.ToString();

            newDto.data.type = "moduleSession";

            return newDto.data;
        }
    }
}