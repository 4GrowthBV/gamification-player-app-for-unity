using System;

namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LoginToken : Attribute, IQueryable
    {
    }
}
