using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SubmitToken : Attribute, IQueryable
    {
    }
}
