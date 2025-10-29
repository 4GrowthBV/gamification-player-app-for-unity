using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatProfile : Attribute, IQueryable
    {
    }
}