using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationResellerEnabled : Attribute, IQueryable
    {
    }
}