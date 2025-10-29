using System;
using System.Collections;
using GamificationPlayer.Session;
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
        public IEnumerator TestUpdateTakeAwaySession()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());
            var dateTime = DateTime.Now;

            return gamificationPlayerEndpoints.CoUpdateTakeAwaySessions(dateTime, dateTime, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestGetTakeAwaySessions()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetTakeAwaySessions((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestCreateTakeAwaySession()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());
            var dateTime = DateTime.Now;

            return gamificationPlayerEndpoints.CoCreateTakeAwaySession(dateTime, dateTime, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestGetMicroGame()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());
            var dateTime = DateTime.Now;

            return gamificationPlayerEndpoints.CoGetMicroGame(Guid.Empty, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestGetMicroGames()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());
            var dateTime = DateTime.Now;

            return gamificationPlayerEndpoints.CoGetMicroGames((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestAppScores()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());
            var dateTime = DateTime.Now;

            return gamificationPlayerEndpoints.CoAppScores(dateTime, dateTime, 888, true, (result, GotoLinkUrl) =>
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
        public IEnumerator TestGetModuleSession()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetModuleSession((result) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);
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

        // Chat Conversations Tests
        [UnityTest]
        public IEnumerator TestGetChatConversations()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetChatConversations((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestGetChatConversation()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetChatConversation(Guid.Empty, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestCreateChatConversation()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoCreateChatConversation((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestDeleteChatConversation()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoDeleteChatConversation(Guid.Empty, (result) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);
            });
        }

        // Chat Messages Tests
        [UnityTest]
        public IEnumerator TestGetChatConversationMessages()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetChatConversationMessages((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestCreateChatConversationMessage()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoCreateChatConversationMessage("user", "Test message", Guid.Empty, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestUpdateChatConversationMessage()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoUpdateChatConversationMessage(Guid.Empty, Guid.Empty, "user", "Updated message", (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestDeleteChatConversationMessage()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoDeleteChatConversationMessage(Guid.Empty, (result) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);
            });
        }

        // Chat Instructions Tests
        [UnityTest]
        public IEnumerator TestGetChatInstructions()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetChatInstructions((result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestGetChatInstruction()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetChatInstruction(Guid.Empty, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestCreateChatInstruction()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoCreateChatInstruction("test_identifier", "Test instruction", (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestUpdateChatInstruction()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoUpdateChatInstruction(Guid.Empty, "Updated instruction", (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestDeleteChatInstruction()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoDeleteChatInstruction(Guid.Empty, (result) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);
            });
        }

        // Chat Predefined Messages Tests
        [UnityTest]
        public IEnumerator TestGetChatPredefinedMessage()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoGetChatPredefinedMessage(Guid.Empty, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestCreateChatPredefinedMessage()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoCreateChatPredefinedMessage("test_identifier", "Test predefined message", new System.Collections.Generic.List<string>{"Button1", "Button2"}, "Test Button", (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestUpdateChatPredefinedMessage()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoUpdateChatPredefinedMessage(Guid.Empty, "Updated predefined message", new System.Collections.Generic.List<string>{"Button1", "Button2"}, "Updated Button", (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestDeleteChatPredefinedMessage()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoDeleteChatPredefinedMessage(Guid.Empty, (result) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);
            });
        }

        // Chat Profile Tests
        [UnityTest]
        public IEnumerator TestCreateChatProfile()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoCreateChatProfile("Test profile name", Guid.Empty, (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestUpdateChatProfile()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoUpdateChatProfile(Guid.Empty, "Updated profile name", (result, dto) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);

                Assert.NotNull(dto);
            });
        }

        [UnityTest]
        public IEnumerator TestDeleteChatProfile()
        {
            var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(gamificationPlayerEnvironmentConfig, new SessionLogDataMock());

            return gamificationPlayerEndpoints.CoDeleteChatProfile(Guid.Empty, (result) =>
            {
                Assert.That(result == UnityWebRequest.Result.Success);
            });
        }
    }
}
