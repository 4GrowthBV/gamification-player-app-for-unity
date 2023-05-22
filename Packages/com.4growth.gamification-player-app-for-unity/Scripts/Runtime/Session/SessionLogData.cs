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

        public bool TryGetLatestLanguage(out string language)
        {
            if(!sessionLogData.TryGetLatestQueryableValue<string, Language>(out var latestLanguage))
            {
                return sessionLogData.TryGetLatestQueryableValue<string, OrganisationDefaultLanguage>(out language);
            }

            language = latestLanguage;

            return true;
        }

        public bool TryGetLatestSubdomain(out string subdomain)
        {
            if(sessionLogData.TryGetLatestQueryableValue<string, OrganisationSubdomain>(out subdomain))
            {
                return true;
            }

            subdomain = PlayerPrefs.GetString("Subdomain");

            return PlayerPrefs.HasKey("Subdomain");
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

        public bool TryGetLatestEnvironmentDomain(out string environmentDomain)
        {
            return sessionLogData.TryGetLatestQueryableValue<string, EnvironmentDomain>(out environmentDomain);
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
            if(TryGetLatestId<OrganisationId>(out id))
            {
                return true;
            }

            if(PlayerPrefs.HasKey("OrganisationId"))
            {
                id = Guid.Parse(PlayerPrefs.GetString("OrganisationId"));
            }
            
            return PlayerPrefs.HasKey("OrganisationId");
        }

        public bool TryGetLatestUserId(out Guid id)
        {
            if(TryGetLatestId<UserId>(out id))
            {
                return true;
            }

            if(PlayerPrefs.HasKey("UserId"))
            {
                id = Guid.Parse(PlayerPrefs.GetString("UserId"));
            }

            return PlayerPrefs.HasKey("UserId");
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

        public void AddToLog(ILoggableData dto, bool clearMissingPersistentData = true)
        {
            sessionLogData.AddToLog(dto);

            SetPersistentData(clearMissingPersistentData);
        }

        public void AddToLog(IEnumerable<ILoggableData> dto, bool clearMissingPersistentData = true)
        {
            sessionLogData.AddToLog(dto);

            SetPersistentData(clearMissingPersistentData);
        }

        private void SetPersistentData(bool clearMissingPersistentData)
        {
            if(sessionLogData.TryGetLatestQueryableValue<string, OrganisationSubdomain>(out var subdomain))
            {
                PlayerPrefs.SetString("Subdomain", subdomain);
            } else if(clearMissingPersistentData)
            {
                PlayerPrefs.DeleteKey("Subdomain");
            }

            if(TryGetLatestId<OrganisationId>(out Guid organisationId))
            {
                PlayerPrefs.SetString("OrganisationId", organisationId.ToString());
            } else if(clearMissingPersistentData)
            {
                PlayerPrefs.DeleteKey("OrganisationId");
            }

            if(TryGetLatestId<UserId>(out Guid userId))
            {
                PlayerPrefs.SetString("UserId", userId.ToString());
            } else if(clearMissingPersistentData)
            {               
                PlayerPrefs.DeleteKey("UserId");
            }
        }

        public void ClearPersistentData()
        {
            PlayerPrefs.DeleteKey("Subdomain");

            PlayerPrefs.DeleteKey("OrganisationId");

            PlayerPrefs.DeleteKey("UserId");
        }
    }
}
