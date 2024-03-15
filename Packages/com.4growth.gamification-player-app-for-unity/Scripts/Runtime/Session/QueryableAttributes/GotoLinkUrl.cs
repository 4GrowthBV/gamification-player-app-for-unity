using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GotoLinkUrl : Attribute, IQueryable
    {
    }
}
