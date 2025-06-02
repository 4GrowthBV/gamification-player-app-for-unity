using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ContextType : Attribute, IQueryable
    {
    }
}
