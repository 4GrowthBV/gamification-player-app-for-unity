using GamificationPlayer.Session;

namespace GamificationPlayer.DTO.Battle
{
    public class TotalOpenBattleInvitationForUserDTO : ILoggableData
    {
        [TotalOpenBattleInvitationForUser]
        public int total;

        public string Type { get => type; }

        public float Time { get; set; }

        public string type = "TotalOpenBattleInvitationForUser";
    }
}
