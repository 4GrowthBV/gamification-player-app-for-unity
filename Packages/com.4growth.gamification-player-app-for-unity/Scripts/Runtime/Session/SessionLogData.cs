using System;
using System.Collections.Generic;
using System.Linq;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.Session;
using UnityEngine;

namespace GamificationPlayer
{
    public class SessionLogData : ISessionLogData
    {
        public IEnumerable<ILoggableData> LogData
        {
            get
            {
                return sessionLogData.LogData;
            }
        }

        private NonPersistentLogData sessionLogData = new NonPersistentLogData();

        public bool TryGetLatestSubdomain(out string subdomain)
        {
            return sessionLogData.TryGetLatestQueryableValue<string, OrganisationSubdomain>(out subdomain);
        }

        public bool TryGetLatestFitnessContentIdentifier(out string fitnessContentIdentifier)
        {
            return sessionLogData.TryGetLatestQueryableValue<string, FitnessContentIdentifier>(out fitnessContentIdentifier);
        }

        public bool TryGetLatestMicroGameIdentifier(out string microGameIdentifier)
        {
            return sessionLogData.TryGetLatestQueryableValue<string, MicroGameIdentifier>(out microGameIdentifier);
        }

        public bool TryGetLatestMicroGamePayload(out MicroGamePayload microGamePayload)
        {
            microGamePayload = LogData.OfType<MicroGamePayload>().LastOrDefault();

            return microGamePayload != null;
        }

        public bool TryGetLatestLoginToken(out string token)
        {
            return sessionLogData.TryGetLatestQueryableValue<string, LoginToken>(out token);
        }

        public bool TryGetWhenServerTime(out float realtimeSinceStartup)
        {
            return sessionLogData.TryGetLatestQueryableValue<float, TimeNowLogged>(out realtimeSinceStartup);
        }

        public bool TryGetLatestModuleSessionId(out Guid id)
        {
            return TryGetLatestId<ModuleSessionId>(out id);
        }

        public bool TryGetLatestModuleSessionStarted(out DateTime dateTime)
        {
            return TryGetLatestDateTime<ModuleSessionStarted>(out dateTime);
        }

        public bool TryGetLatestModuleSessionEnded(out DateTime dateTime)
        {
            return TryGetLatestDateTime<ModuleSessionEnded>(out dateTime);
        }

        public bool TryGetLatestModuleSessionCompleted(out DateTime dateTime)
        {
            return TryGetLatestDateTime<ModuleSessionCompleted>(out dateTime);
        }

        public bool TryGetLatestServerTime(out DateTime dateTime)
        {
            return TryGetLatestDateTime<TimeNow>(out dateTime);
        }

        public bool TryGetLatestModuleId(out Guid id)
        {           
            return TryGetLatestId<ModuleId>(out id);
        }

        public bool TryGetLatestChallengeSessionId(out Guid id)
        {
            return TryGetLatestId<ChallengeSessionId>(out id);
        }

        public bool TryGetLatestDeviceFlowId(out Guid id)
        {
            return TryGetLatestId<DeviceFlowId>(out id);
        }

        public bool TryGetLatestOrganisationId(out Guid id)
        {
            return TryGetLatestId<OrganisationId>(out id);
        }

        public bool TryGetLatestUserId(out Guid id)
        {
            return TryGetLatestId<UserId>(out id);
        }

        private bool TryGetLatestId<TAttribute>(out Guid id)
            where TAttribute : Session.IQueryable
        {
            if(sessionLogData.TryGetLatestQueryableValue<string, TAttribute>(out var idString))
            {
                return Guid.TryParse(idString, out id);
            }

            id = default;

            return false;
        }

        private bool TryGetLatestDateTime<TAttribute>(out DateTime dateTime)
            where TAttribute : Session.IQueryable
        {
            if(sessionLogData.TryGetLatestQueryableValue<string, TAttribute>(out var dateTimeString))
            {
                if(dateTimeString == null)
                {
                    dateTime = default;

                    return false;
                }
                return DateTime.TryParse(dateTimeString, out dateTime);
            }

            dateTime = default;

            return false;
        }

        public void AddToLog(ILoggableData dto)
        {
            sessionLogData.AddToLog(dto);
        }

        public void AddToLog(IEnumerable<ILoggableData> dto)
        {
            sessionLogData.AddToLog(dto);
        }
    }
}
