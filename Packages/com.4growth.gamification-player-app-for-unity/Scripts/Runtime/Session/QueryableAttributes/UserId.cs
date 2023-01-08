using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UserId : Attribute, IQueryable
    {
    }
}
