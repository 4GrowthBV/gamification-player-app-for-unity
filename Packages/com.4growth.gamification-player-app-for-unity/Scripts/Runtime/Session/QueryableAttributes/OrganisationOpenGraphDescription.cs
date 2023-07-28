using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationOpenGraphDescription : Attribute, IQueryable
    {
    }
}
