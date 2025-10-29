using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatRole : Attribute, IQueryable
    {
    }
}