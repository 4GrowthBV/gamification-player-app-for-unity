using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationAllowUpgradeToRegisteredUser : Attribute, IQueryable
    {
    }
}