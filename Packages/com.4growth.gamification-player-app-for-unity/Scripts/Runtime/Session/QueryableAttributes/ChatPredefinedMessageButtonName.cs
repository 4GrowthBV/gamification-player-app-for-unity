using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatPredefinedMessageButtonName : Attribute, IQueryable
    {
    }
}