using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnvironmentType : Attribute, IQueryable
    {
    }
}
