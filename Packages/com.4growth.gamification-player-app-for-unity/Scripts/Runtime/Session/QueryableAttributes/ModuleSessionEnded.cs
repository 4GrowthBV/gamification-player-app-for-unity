using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ModuleSessionEnded : Attribute, IQueryable
    {
    }
}
