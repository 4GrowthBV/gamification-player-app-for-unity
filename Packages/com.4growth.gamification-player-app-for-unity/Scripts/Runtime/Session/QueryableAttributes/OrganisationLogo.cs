using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationLogo : Attribute, IQueryable
    {
    }
}
