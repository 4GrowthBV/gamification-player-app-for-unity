using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationAnonymousEnabled : Attribute, IQueryable
    {
    }
}
