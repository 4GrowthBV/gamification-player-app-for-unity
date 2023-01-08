using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ModuleSessionStarted : Attribute, IQueryable
    {
    }
}
