using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationPrimaryColor : Attribute, IQueryable
    {
    }
}
