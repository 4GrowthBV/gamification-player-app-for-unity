using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationDefaultLanguage : Attribute, IQueryable
    {
    }
}
