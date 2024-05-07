using System;
using System.Linq;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.DTO.LoginToken;
using GamificationPlayer.DTO.ModuleSession;
using GamificationPlayer.Session;
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
        public void TestTryGetLatestQueryableBoolSpecialCase()
        {
            var json = "{\"data\":{\"type\":\"pageView\",\"attributes\":{\"organisation_id\":\"edb5e165-1c74-44f8-8d57-c24b82f2f5f2\",\"organisation_allow_upgrade_to_registered_user\":null,\"user_id\":null,\"user_is_demo\":null,\"language\":\"nl\"}}}";
        
            var dto = json.FromJson<PageViewDTO>();

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);

            Assert.That(sessionData.TryGetLatest<UserIsDemo>(out bool _));
            if(sessionData.TryGetLatest<UserIsDemo>(out bool isUserDemo))
            {
                Assert.AreEqual(isUserDemo, false);
            }

            Assert.That(sessionData.TryGetLatest<OrganisationAllowUpgradeToRegisteredUser>(out bool _));
            if(sessionData.TryGetLatest<OrganisationAllowUpgradeToRegisteredUser>(out bool organisationAllowUpgradeToRegisteredUser))
            {
                Assert.AreEqual(organisationAllowUpgradeToRegisteredUser, false);
            }
        }

        [Test]
        public void TestTryGetLatestUserTagsViaPageView()
        {
            var dto = new PageViewDTO();
            dto.data.attributes = new PageViewDTO.Attributes
            {
                user_tags = new string[] { "tag1", "tag2" }
            };

            dto.data.type = "pageView";

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);

            Assert.That(sessionData.TryGetLatest<UserTags>(out string[] _));
            if(sessionData.TryGetLatest<UserTags>(out string[] tags))
            {
                var index = 0;
                foreach (var tag in tags)
                {
                    Assert.That(tag == dto.data.attributes.user_tags[index]);
                    index++;
                }
            }
        }

        [Test]
        public void TestTryGetLatestUserTagsViaAPI()
        {
            var dto = new GetUserResponseDTO();

            dto.data.attributes.name = "John Doe";

            dto.included = new GetUserResponseDTO.Tags[]
            {
                new GetUserResponseDTO.Tags
                {
                    attributes = new GetUserResponseDTO.Tags.Attributes
                    {
                        name = "tag1"
                    }
                },
                new GetUserResponseDTO.Tags
                {
                    attributes = new GetUserResponseDTO.Tags.Attributes
                    {
                        name = "tag2"
                    }
                }
            };

            dto.data.type = "user";
            
            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);
            sessionData.AddToLog(new UserTagsDataHelper(dto));

            Assert.That(sessionData.TryGetLatest<UserTags>(out string[] _));
            if(sessionData.TryGetLatest<UserTags>(out string[] tags))
            {
                var index = 0;
                foreach (var tag in tags)
                {
                    Assert.That(tag == dto.included[index].attributes.name);
                    index++;
                }
            }
        }

        [Test]
        public void TestTryGetLatestQueryableBoolValue()
        {
            var dto = new PageViewDTO();

            dto.data.attributes.organisation_allow_upgrade_to_registered_user = true;
            dto.data.attributes.user_is_demo = false;

            dto.data.type = "pageView";

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto.data);

            Assert.That(sessionData.TryGetLatest<UserIsDemo>(out bool _));
            if(sessionData.TryGetLatest<UserIsDemo>(out bool isUserDemo))
            {
                Assert.AreEqual(isUserDemo, dto.data.attributes.user_is_demo);
            }

            Assert.That(sessionData.TryGetLatest<OrganisationAllowUpgradeToRegisteredUser>(out bool _));
            if(sessionData.TryGetLatest<OrganisationAllowUpgradeToRegisteredUser>(out bool organisationAllowUpgradeToRegisteredUser))
            {
                Assert.AreEqual(organisationAllowUpgradeToRegisteredUser, dto.data.attributes.organisation_allow_upgrade_to_registered_user);
            }
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
            dto.data.relationships.challenge_session.data.id = Guid.NewGuid().ToString();
            dto.data.relationships.module.data.id = Guid.NewGuid().ToString();

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

            var sessionData = new SessionLogData();

            sessionData.AddToLog(dto0.data);
            sessionData.AddToLog(dto1.data);
            sessionData.AddToLog(dto2.data);  
            sessionData.AddToLog(dto3.data);  
            sessionData.AddToLog(dto4.data);
            sessionData.AddToLog(lastDTO.data);  

            Assert.That(sessionData.LogData.Count() == 6);

            Assert.That(sessionData.TryGetLatestMicroGameIdentifier(out _));
            if(sessionData.TryGetLatestMicroGameIdentifier(out var identifier))
            {
                Assert.AreEqual(dto0.data.attributes.identifier, identifier);
            }

            Assert.That(sessionData.TryGetLatestModuleSessionId(out _));
            if(sessionData.TryGetLatestModuleSessionId(out var id))
            {
                Assert.AreEqual(Guid.Parse(dto4.data.id), id);
            }

            Assert.That(sessionData.TryGetLatestModuleId(out _));
            if(sessionData.TryGetLatestModuleId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto4.data.relationships.module.data.id), id);
            }

            Assert.That(sessionData.TryGetLatestChallengeSessionId(out _));
            if(sessionData.TryGetLatestChallengeSessionId(out id))
            {
                Assert.AreEqual(Guid.Parse(dto4.data.relationships.challenge_session.data.id), id);
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
