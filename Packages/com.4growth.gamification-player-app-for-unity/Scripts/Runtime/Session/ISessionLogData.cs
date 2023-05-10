using System;
using System.Collections.Generic;

namespace GamificationPlayer
{
    public interface ISessionLogData
    {
        public bool TryGetLatestSubdomain(out string subdomain);

        public bool TryGetLatestMicroGameIdentifier(out string identifier);

        public bool TryGetLatestLoginToken(out string token);

        public bool TryGetLatestModuleSessionId(out Guid id);

        public bool TryGetLatestModuleSessionStarted(out DateTime dateTime);

        public bool TryGetLatestModuleSessionEnded(out DateTime dateTime);

        public bool TryGetLatestModuleSessionCompleted(out DateTime dateTime);

        public bool TryGetLatestServerTime(out DateTime dateTime);

        public bool TryGetWhenServerTime(out float realtimeSinceStartup);
        
        public bool TryGetLatestModuleId(out Guid id);

        public bool TryGetLatestChallengeSessionId(out Guid id);
        public bool TryGetLatestDeviceFlowId(out Guid id);

        public bool TryGetLatestOrganisationId(out Guid id);

        public bool TryGetLatestUserId(out Guid id);

        public void AddToLog(ILoggableData dto);

        public void AddToLog(IEnumerable<ILoggableData> dto);
    }
}