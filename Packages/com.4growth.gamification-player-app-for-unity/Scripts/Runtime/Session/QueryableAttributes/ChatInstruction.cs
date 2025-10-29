using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatInstruction : Attribute, IQueryable
    {
    }
}