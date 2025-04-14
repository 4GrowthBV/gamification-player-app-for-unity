using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UserRole : Attribute, IQueryable
    {
    }
}
