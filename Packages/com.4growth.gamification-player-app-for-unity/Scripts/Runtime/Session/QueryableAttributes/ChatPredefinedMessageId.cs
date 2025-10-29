using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatPredefinedMessageId : Attribute, IQueryable
    {
    }
}