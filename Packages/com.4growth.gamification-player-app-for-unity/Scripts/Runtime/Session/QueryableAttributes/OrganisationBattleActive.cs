using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationBattleActive : Attribute, IQueryable
    {
    }
}
