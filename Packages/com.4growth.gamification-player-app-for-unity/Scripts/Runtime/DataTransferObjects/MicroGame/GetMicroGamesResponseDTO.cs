using System;

namespace GamificationPlayer.DTO.MicroGame
{
    public class GetMicroGamesResponseDTO
    {
        public GetMicroGameResponseDTO.Data[] data;

        public GetMicroGamesResponseDTO()
        {
            data = Array.Empty<GetMicroGameResponseDTO.Data>();
        }
    }
}
