using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UserBattleScore : Attribute, IQueryable
    {
    }
}
