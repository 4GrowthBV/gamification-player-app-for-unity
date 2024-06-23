using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MicroGameScore : Attribute, IQueryable
    {
    }
}
