using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationSubdomain : Attribute, IQueryable
    {
    }
}
