using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BattleAvailableFrom : Attribute, IQueryable
    {
    }
}
