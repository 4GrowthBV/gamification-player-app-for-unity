using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.DTO.LoginToken;
using GamificationPlayer.DTO.ModuleSession;
using GamificationPlayer.Session;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class NonPersistentLogDataTest
    {
        [Test]
        public void TestAdd()
        {
            var dto = new StandardDTO();

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.LogData.Count() == 1);
        }

        [Test]
        public void TestAddMultiple()
        {
            var dto1 = new StandardDTO();
            var dto2 = new StandardDTO();

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(new ILoggableData[] { dto1.data, dto2.data });

            Assert.That(nonPersistentSessionData.LogData.Count() == 2);
        }

        [Test]
        public void TestTryGetLatestQueryableIdValue()
        {
            var dto = new ModuleSessionStartedDTO();

            dto.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSessionStarted";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out var id))
            {
                Assert.AreEqual(dto.data.attributes.challenge_session_id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out id))
            {
                Assert.AreEqual(dto.data.attributes.module_id, id);
            }


            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out id))
            {
                Assert.AreEqual(dto.data.attributes.module_session_id, id);
            }
        }

        [Test]
        public void TestTryGetLatestQueryableDateTimeValue()
        {
            var dto = new UpdateModuleSessionResponseDTO();

            dto.data.id = Guid.NewGuid().ToString();
            dto.data.attributes.started_at = new DateTime(2000, 1, 1).ToString();
            dto.data.attributes.ended_at = new DateTime(2001, 1, 1).ToString();
            dto.data.attributes.completed_at = "null";
            dto.data.relationships.challenge_session.data.id = Guid.NewGuid().ToString();
            dto.data.relationships.module.data.id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSession";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out var moduleId))
            {
                Assert.AreEqual(dto.data.relationships.module.data.id, moduleId);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out var challengeSessionId))
            {
                Assert.AreEqual(dto.data.relationships.challenge_session.data.id, challengeSessionId);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out var dateTime))
            {
                Assert.AreEqual(dto.data.attributes.started_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out dateTime))
            {
                Assert.AreEqual(dto.data.attributes.ended_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out dateTime))
            {
                Assert.AreEqual(dto.data.attributes.completed_at, dateTime);
            }
        }

        [Test]
        public void TestTryGetLatestQueryableTokenValue()
        {
            var dto = new GetLoginTokenResponseDTO();

            dto.data.attributes.token = "123456789";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out var id))
            {
                Assert.AreEqual(dto.data.attributes.token, id);
            }
        }   

        [Test]
        public void TestTryGetLatestQueryableValueWithMultiple()
        {
            var dto00 = new MicroGameOpenedDTO();

            dto00.data.type = "fitnessContentOpened";
            dto00.data.attributes.identifier = Guid.NewGuid().ToString();

            var dto0 = new MicroGameOpenedDTO();

            dto0.data.type = "microGameOpened";
            dto0.data.attributes.identifier = Guid.NewGuid().ToString();


            var dto1 = new ModuleSessionStartedDTO();

            dto1.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto1.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto1.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto1.data.attributes.module_id = Guid.NewGuid().ToString();
            dto1.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto1.data.type = "moduleSessionStarted";

            var dto2 = new GetLoginTokenResponseDTO();

            dto2.data.attributes.token = "123456789";
            dto2.data.type = "login_token";

            var dto3 = new ModuleSessionStartedDTO();

            dto3.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto3.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto3.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto3.data.attributes.module_id = Guid.NewGuid().ToString();
            dto3.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto3.data.type = "moduleSessionStarted";

            var dto4 = new UpdateModuleSessionResponseDTO();

            dto4.data.id = Guid.NewGuid().ToString();
            dto4.data.attributes.started_at = new DateTime(2000, 1, 1).ToString();
            dto4.data.attributes.ended_at = new DateTime(2001, 1, 1).ToString();
            dto4.data.attributes.completed_at = "null";
            dto4.data.relationships.challenge_session.data.id = Guid.NewGuid().ToString();
            dto4.data.relationships.module.data.id = Guid.NewGuid().ToString();

            dto4.data.type = "moduleSession";

            var lastDTO = new PageViewDTO();

            lastDTO.data.attributes.organisation_id = Guid.NewGuid().ToString();
            lastDTO.data.attributes.user_id = Guid.NewGuid().ToString();

            lastDTO.data.type = "pageView";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(new ILoggableData[] { dto00.data, dto0.data, dto1.data, dto2.data, dto3.data, dto4.data, lastDTO.data });    

            Assert.That(nonPersistentSessionData.LogData.Count() == 7);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, MicroGameIdentifier>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, MicroGameIdentifier>(out var id))
            {
                Assert.AreEqual(dto0.data.attributes.identifier, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, UserId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, UserId>(out id))
            {
                Assert.AreEqual(lastDTO.data.attributes.user_id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, OrganisationId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, OrganisationId>(out id))
            {
                Assert.AreEqual(lastDTO.data.attributes.organisation_id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out id))
            {
                Assert.AreEqual(dto4.data.relationships.challenge_session.data.id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out id))
            {
                Assert.AreEqual(dto4.data.relationships.module.data.id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out id))
            {
                Assert.AreEqual(dto4.data.id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out id))
            {
                Assert.AreEqual(dto2.data.attributes.token, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out var dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.started_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.ended_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.completed_at, dateTime);
            }
        }
    }
}
