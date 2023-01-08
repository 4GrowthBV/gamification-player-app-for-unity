using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OrganisationWebhookUrl : Attribute, IQueryable
    {
    }
}
