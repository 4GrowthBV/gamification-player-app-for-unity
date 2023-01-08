using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChallengeSessionId : Attribute, IQueryable
    {
    }
}
