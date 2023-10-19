using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TotalOpenBattleInvitationForUser : Attribute, IQueryable
    {
    }
}
