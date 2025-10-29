using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatMessage : Attribute, IQueryable
    {
    }
}