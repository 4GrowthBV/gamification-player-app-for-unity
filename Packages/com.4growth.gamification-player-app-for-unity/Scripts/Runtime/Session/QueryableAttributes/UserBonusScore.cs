using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UserBonusScore : Attribute, IQueryable
    {
    }
}
