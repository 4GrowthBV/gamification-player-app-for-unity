using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TakeAwaySessionId : Attribute, IQueryable
    {
    }
}
