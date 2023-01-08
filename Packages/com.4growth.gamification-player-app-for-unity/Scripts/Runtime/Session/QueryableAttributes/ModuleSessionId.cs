using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ModuleSessionId : Attribute, IQueryable
    {
    }
}
