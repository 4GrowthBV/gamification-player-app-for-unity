using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MicroGameJSONWebToken : Attribute, IQueryable
    {
    }
}
