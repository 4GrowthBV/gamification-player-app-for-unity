using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationResellerID : Attribute, IQueryable
    {
    }
}
