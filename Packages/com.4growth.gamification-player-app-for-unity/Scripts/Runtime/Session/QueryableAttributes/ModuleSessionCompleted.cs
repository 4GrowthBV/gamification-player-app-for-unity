using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ModuleSessionCompleted : Attribute, IQueryable
    {
    }
}
