using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UserIsDemo : Attribute, IQueryable
    {
    }
}
