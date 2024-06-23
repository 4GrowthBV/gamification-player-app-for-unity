using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MicroGameCompletedAt : Attribute, IQueryable
    {
    }
}
