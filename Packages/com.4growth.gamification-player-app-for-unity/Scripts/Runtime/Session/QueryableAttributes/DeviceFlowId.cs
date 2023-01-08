using System;
namespace GamificationPlayer.Session
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DeviceFlowId : Attribute, IQueryable
    {
    }
}
