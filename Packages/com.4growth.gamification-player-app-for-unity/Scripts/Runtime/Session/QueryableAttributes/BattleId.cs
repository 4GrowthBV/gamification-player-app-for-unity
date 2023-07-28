using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BattleId : Attribute, IQueryable
    {
    }
}
