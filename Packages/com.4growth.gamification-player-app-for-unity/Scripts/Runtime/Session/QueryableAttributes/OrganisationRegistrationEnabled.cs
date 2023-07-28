using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationRegistrationEnabled : Attribute, IQueryable
    {
    }
}
