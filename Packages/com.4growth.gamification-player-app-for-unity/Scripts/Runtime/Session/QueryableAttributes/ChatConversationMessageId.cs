using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatConversationMessageId : Attribute, IQueryable
    {
    }
}