using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ChatInstructionId : Attribute, IQueryable
    {
    }
}