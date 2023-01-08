using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.Session;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class SessionLogDataMock : ISessionLogData
    {
        public void AddToLog(ILoggableData dto)
        {
        }

        public void AddToLog(IEnumerable<ILoggableData> dto)
        {
        }

        public bool TryGetLatestChallengeSessionId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestDeviceFlowId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestSubdomain(out string subdomain)
        {
            subdomain = Guid.NewGuid().ToString();

            return true;
        }

        public bool TryGetLatestFitnessContentIdentifier(out string identifier)
        {
            identifier = Guid.NewGuid().ToString();

            return true;
        }

        public bool TryGetLatestId<TAttribute>(out Guid id) where TAttribute : IQueryable
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestLoginToken(out string token)
        {
            token = "123";

            return true;
        }

        public bool TryGetLatestMicroGameIdentifier(out string identifier)
        {
            identifier = Guid.NewGuid().ToString();

            return true;
        }

        public bool TryGetLatestModuleId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestModuleSessionCompleted(out DateTime dateTime)
        {
            dateTime = default;

            return false;
        }

        public bool TryGetLatestModuleSessionEnded(out DateTime dateTime)
        {
            dateTime = default;

            return false;
        }

        public bool TryGetLatestModuleSessionId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestModuleSessionStarted(out DateTime dateTime)
        {
            dateTime = new DateTime(2000, 1, 1);

            return true;
        }

        public bool TryGetLatestOrganisationId(out Guid id)
        {
            id = new Guid("7bcfc94d-8a06-4fa8-b5ff-b35415a65b16");

            return true;
        }

        public bool TryGetLatestServerTime(out DateTime dateTime)
        {
            dateTime = DateTime.Now;

            return true;
        } 

        public bool TryGetLatestUserId(out Guid id)
        {
            id = new Guid("46f1d6fc-36b0-48fe-8ffd-e8dfc1a15eba");

            return true;
        }

        public bool TryGetWhenServerTime(out float realtimeSinceStartup)
        {
            realtimeSinceStartup = 0f;

            return true;
        }
    }
}
