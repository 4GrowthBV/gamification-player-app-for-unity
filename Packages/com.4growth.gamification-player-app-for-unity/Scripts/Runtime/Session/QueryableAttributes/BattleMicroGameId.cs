using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BattleMicroGameId : Attribute, IQueryable
    {
    }
}
