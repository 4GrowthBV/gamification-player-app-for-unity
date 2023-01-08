using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeNow : Attribute, IQueryable
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class TimeNowLogged : Attribute, IQueryable
    {
    }
}
