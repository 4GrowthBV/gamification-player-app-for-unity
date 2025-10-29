using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatPredefinedMessageIdentifier : Attribute, IQueryable
    {
    }
}