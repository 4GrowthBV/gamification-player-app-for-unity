using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatPredefinedMessageContent : Attribute, IQueryable
    {
    }
}