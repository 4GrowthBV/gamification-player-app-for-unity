using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnvironmentDomain : Attribute, IQueryable
    {
    }
}
