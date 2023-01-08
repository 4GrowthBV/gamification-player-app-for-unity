using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.DTO.LoginToken;
using GamificationPlayer.DTO.ModuleSession;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class SessionLogDataTest
    {
        [Test]
        public void TestAdd()
        {
            var dto = new StandardDTO();

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);

            Assert.That(sessionData.LogData.Count() == 1);
        }

        [Test]
        public void TestAddMultiple()
        {
            var dto1 = new StandardDTO();
            var dto2 = new StandardDTO();

            var sessionData = new SessionLogData();

            sessionData.AddToLog(new ILoggableData[] { dto1.data, dto2.data });

            Assert.That(sessionData.LogData.Count() == 2);
        }

        [Test]
        public void TestTryGetLatestQueryableIdValue()
        {
            var dto = new ModuleSessionStartedDTO();

            dto.data.attributes.organisation_id = Guid.NewGuid().ToString();
            dto.data.attributes.user_id = Guid.NewGuid().ToString();
            dto.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSessionStarted";

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);

            Assert.That(sessionData.TryGetLatestModuleSessionId(out _));
            if(sessionData.TryGetLatestModuleSessionId(out var id))
            {
                Assert.AreEqual(Guid.Parse(dto.data.attributes.module_session_id), id);
            }

            Assert.That(sessionData.TryGetLatestModuleId(out _));
            if(sessionData.TryGetLatestModuleId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto.data.attributes.module_id), id);
            }

            Assert.That(sessionData.TryGetLatestChallengeSessionId(out _));
            if(sessionData.TryGetLatestChallengeSessionId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto.data.attributes.challenge_session_id), id);
            }

            Assert.That(sessionData.TryGetLatestOrganisationId(out _));
            if(sessionData.TryGetLatestOrganisationId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto.data.attributes.organisation_id), id);
            }

            Assert.That(sessionData.TryGetLatestUserId(out _));
            if(sessionData.TryGetLatestUserId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto.data.attributes.user_id), id);
            }
        }

        [Test]
        public void TestTryGetLatestQueryableDateTimeValue()
        {
            var dto = new UpdateModuleSessionResponseDTO();

            var startedAt = new DateTime(2000, 1, 1);
            var endedAt = new DateTime(2001, 1, 1);

            dto.data.id = Guid.NewGuid().ToString();
            dto.data.attributes.started_at = startedAt.ToString();
            dto.data.attributes.ended_at = endedAt.ToString();
            dto.data.attributes.completed_at = "null";
            dto.data.relationships.challenge.data.id = Guid.NewGuid().ToString();
            dto.data.relationships.user.data.id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSession";

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);

            Assert.That(sessionData.TryGetLatestModuleSessionStarted(out _));
            if(sessionData.TryGetLatestModuleSessionStarted(out var dateTime))
            {
                Assert.AreEqual(startedAt, dateTime);
            }

            Assert.That(sessionData.TryGetLatestModuleSessionEnded(out _));
            if(sessionData.TryGetLatestModuleSessionEnded(out dateTime))
            {
                Assert.AreEqual(endedAt, dateTime);
            }

            Assert.IsFalse(sessionData.TryGetLatestModuleSessionCompleted(out _));
        }

        [Test]
        public void TestTryGetLatestQueryableTokenValue()
        {
            var dto = new GetLoginTokenResponseDTO();

            dto.data.attributes.token = "123456789";

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);

            Assert.That(sessionData.TryGetLatestLoginToken(out _));
            if(sessionData.TryGetLatestLoginToken(out var token))
            {
                Assert.AreEqual(dto.data.attributes.token, token);
            }
        }

        [Test]
        public void TestTryGetLatestQueryableValueWithMultiple()
        {
            var dto00 = new FitnessContentOpenedDTO();

            dto00.data.type = "fitnessContentOpened";
            dto00.data.attributes.identifier = Guid.NewGuid().ToString();

            var dto0 = new MicroGameOpenedDTO();

            dto0.data.type = "microGameOpened";
            dto0.data.attributes.identifier = Guid.NewGuid().ToString();

            var dto1 = new ModuleSessionStartedDTO();

            dto1.data.attributes.organisation_id = Guid.NewGuid().ToString();
            dto1.data.attributes.user_id = Guid.NewGuid().ToString();
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

            dto3.data.attributes.organisation_id = Guid.NewGuid().ToString();
            dto3.data.attributes.user_id = Guid.NewGuid().ToString();
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
            dto4.data.relationships.challenge.data.id = Guid.NewGuid().ToString();
            dto4.data.relationships.user.data.id = Guid.NewGuid().ToString();

            dto4.data.type = "moduleSession";

            var lastDTO = new PageViewDTO();

            lastDTO.data.attributes.organisation_id = Guid.NewGuid().ToString();
            lastDTO.data.attributes.user_id = Guid.NewGuid().ToString();

            lastDTO.data.type = "pageView";

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto0.data);
            sessionData.AddToLog(dto00.data);
            sessionData.AddToLog(dto1.data);
            sessionData.AddToLog(dto2.data);  
            sessionData.AddToLog(dto3.data);  
            sessionData.AddToLog(dto4.data);
            sessionData.AddToLog(lastDTO.data);  

            Assert.That(sessionData.LogData.Count() == 7);

            Assert.That(sessionData.TryGetLatestMicroGameIdentifier(out _));
            if(sessionData.TryGetLatestMicroGameIdentifier(out var identifier))
            {
                Assert.AreEqual(dto0.data.attributes.identifier, identifier);
            }

            Assert.That(sessionData.TryGetLatestFitnessContentIdentifier(out _));
            if(sessionData.TryGetLatestFitnessContentIdentifier(out identifier))
            {
                Assert.AreEqual(dto00.data.attributes.identifier, identifier);
            }

            Assert.That(sessionData.TryGetLatestModuleSessionId(out _));
            if(sessionData.TryGetLatestModuleSessionId(out var id))
            {
                Assert.AreEqual(Guid.Parse(dto4.data.id), id);
            }

            Assert.That(sessionData.TryGetLatestModuleId(out _));
            if(sessionData.TryGetLatestModuleId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto3.data.attributes.module_id), id);
            }

            Assert.That(sessionData.TryGetLatestChallengeSessionId(out _));
            if(sessionData.TryGetLatestChallengeSessionId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto3.data.attributes.challenge_session_id), id);
            }

            Assert.That(sessionData.TryGetLatestOrganisationId(out _));
            if(sessionData.TryGetLatestOrganisationId(out id))
            {
                Assert.AreEqual(Guid.Parse(lastDTO.data.attributes.organisation_id), id);
            }

            Assert.That(sessionData.TryGetLatestUserId(out _));
            if(sessionData.TryGetLatestUserId(out id))
            {
                Assert.AreEqual(Guid.Parse(lastDTO.data.attributes.user_id), id);
            }

            Assert.That(sessionData.TryGetLatestLoginToken(out _));
            if(sessionData.TryGetLatestLoginToken(out var token))
            {
                Assert.AreEqual(dto2.data.attributes.token, token);
            }

            Assert.That(sessionData.TryGetLatestModuleSessionStarted(out _));
            if(sessionData.TryGetLatestModuleSessionStarted(out var dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.StartedAt, dateTime);
            }

            Assert.That(sessionData.TryGetLatestModuleSessionEnded(out _));
            if(sessionData.TryGetLatestModuleSessionEnded(out dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.EndedAt, dateTime);
            }

            Assert.IsFalse(sessionData.TryGetLatestModuleSessionCompleted(out _));
        }   
    }
}
