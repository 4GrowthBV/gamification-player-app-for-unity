using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MicroGameIdentifier : Attribute, IQueryable
    {
    }
}
