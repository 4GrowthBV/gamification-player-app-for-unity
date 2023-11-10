using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GamificationPlayerEndpointsTest
    {
        private EnvironmentConfig gamificationPlayerEnvironmentConfig;

        [SetUp]
        public void SetUp()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out gamificationPlayerEnvironmentConfig);
        }

        [UnityTest]
        public IEnumerator TestAppScores()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());
            var dateTime = DateTime.Now;

            return gamificationPlayerEndpoints.CoAppScores(dateTime, 888, true, (result) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);
            });
        }

        [UnityTest]
        public IEnumerator TestAnnounceDeviceFlow()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoAnnounceDeviceFlow((result, loginUrl) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(!string.IsNullOrEmpty(loginUrl));
            });
        }

        [UnityTest]
        public IEnumerator TestGetDeviceFlow()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetDeviceFlow((result, isValidated, userId) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(isValidated);

                Assert.That(!string.IsNullOrEmpty(userId));
            });
        }

        [UnityTest]
        public IEnumerator TestGetLoginToken()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetLoginToken((result, token) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(!string.IsNullOrEmpty(token));
            });
        }

        [UnityTest]
        public IEnumerator TestGetModuleSessionId()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetModuleSessionId((result, moduleSessionId) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(moduleSessionId != Guid.Empty);
            });
        }

        [UnityTest]
        public IEnumerator TestGetTime()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetTime((result, time) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(time != default);
            });
        }

        [UnityTest]
        public IEnumerator TestGetUser()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetUser((result, userDTO) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(userDTO != default);
            });
        }

        [UnityTest]
        public IEnumerator TestGetOrganisation()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetOrganisation((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(dto != null);
            });
        }

        [UnityTest]
        public IEnumerator TestGetOpenBattleInvitationsForUser()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetOpenBattleInvitationsForUser((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.That(dto == 1);
            });
        }
    }
}
