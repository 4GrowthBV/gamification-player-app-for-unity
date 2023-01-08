using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChallengeId : Attribute, IQueryable
    {
    }
}
