using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UserScore : Attribute, IQueryable
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UserTags : Attribute, IQueryable
    {
    }
}
