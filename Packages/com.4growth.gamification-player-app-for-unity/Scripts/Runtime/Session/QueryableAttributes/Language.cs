using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Language : Attribute, IQueryable
    {
    }
}
