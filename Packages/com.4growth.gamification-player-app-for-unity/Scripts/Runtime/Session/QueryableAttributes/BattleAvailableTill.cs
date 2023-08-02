using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BattleAvailableTill : Attribute, IQueryable
    {
    }
}
