using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationIntroduction : Attribute, IQueryable
    {
    }
}
