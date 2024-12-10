using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BattleOpponentAvatar : Attribute, IQueryable
    {
    }
}
