using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BattleOpponentName : Attribute, IQueryable
    {
    }
}
