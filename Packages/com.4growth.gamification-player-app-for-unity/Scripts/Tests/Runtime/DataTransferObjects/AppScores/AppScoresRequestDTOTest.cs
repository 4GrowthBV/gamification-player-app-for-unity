using NUnit.Framework;
using GamificationPlayer.DTO.AppScores;
using System;
using UnityEngine;
using GamificationPlayer.DTO.ExternalEvents;

namespace GamificationPlayer.Tests
{
    public class AppScoresRequestDTOTest
    {
        [SetUp]
        public void Setup()
        {
            // We had some problems with parsing the date for Indonesian players, 
            // so by removing the comments we can set the culture to Indonesian during the tests
            // System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("id-ID");
        }

        [Test]
        public void TestConstructor()
        {
            var userId = Guid.NewGuid();
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);
            var integration = new MicroGamePayload.Integration
            {
                id = Guid.NewGuid().ToString(),
                context = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "key", "value" }
                }
            };

            var obj = AppScoresRequestDTO.CreateBattleSessionRequest(userId, date, date, score, date, integration);

            Assert.AreEqual(userId.ToString(), obj.Data.Attributes.UserId);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.StartedAt);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(integration.id, obj.Data.Attributes.Integration.id);


            obj = AppScoresRequestDTO.CreateModuleSessionRequest(date, score, date, integration);

            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.CompletedAt);
            Assert.AreEqual(integration.id, obj.Data.Attributes.Integration.id);

            obj = AppScoresRequestDTO.CreateDailyChallengeRequest(userId, date, date, score, date, integration);

            Assert.AreEqual(userId.ToString(), obj.Data.Attributes.UserId);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.StartedAt);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.CompletedAt);
            Assert.AreEqual(integration.id, obj.Data.Attributes.Integration.id);

            obj = AppScoresRequestDTO.CreateDirectPlayRequest(userId, date, date, score, date, integration);

            Assert.AreEqual(userId.ToString(), obj.Data.Attributes.UserId);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.StartedAt);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.CompletedAt);
            Assert.AreEqual(integration.id, obj.Data.Attributes.Integration.id);
        }

        [Test]
        public void TestNullConstructor()
        {
            var userId = Guid.NewGuid();
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);

            var obj = AppScoresRequestDTO.CreateBattleSessionRequest(userId, date, date, score, null, null);
            Assert.AreEqual(userId.ToString(), obj.Data.Attributes.UserId);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.StartedAt);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(null, obj.Data.Attributes.CompletedAt);
            Assert.AreEqual(null, obj.Data.Attributes.Integration);

            obj = AppScoresRequestDTO.CreateModuleSessionRequest(date, score, null, null);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(null, obj.Data.Attributes.CompletedAt);
            Assert.AreEqual(null, obj.Data.Attributes.Integration);

            obj = AppScoresRequestDTO.CreateDailyChallengeRequest(userId, date, date, score, null, null);
            Assert.AreEqual(userId.ToString(), obj.Data.Attributes.UserId);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.StartedAt);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(null, obj.Data.Attributes.CompletedAt);
            Assert.AreEqual(null, obj.Data.Attributes.Integration);

            obj = AppScoresRequestDTO.CreateDirectPlayRequest(userId, date, date, score, null, null);
            Assert.AreEqual(userId.ToString(), obj.Data.Attributes.UserId);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.StartedAt);
            Assert.AreEqual(date.ToString("yyyy-MM-ddTHH:mm:ssZ"), obj.Data.Attributes.EndedAt);
            Assert.AreEqual(score, obj.Data.Attributes.Score);
            Assert.AreEqual(null, obj.Data.Attributes.CompletedAt);
            Assert.AreEqual(null, obj.Data.Attributes.Integration);
        }

        [Test]
        public void TestToJSON()
        {
            var userId = Guid.NewGuid();
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);
            var integration = new MicroGamePayload.Integration
            {
                id = Guid.NewGuid().ToString(),
                context = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "key", "value" }
                }
            };

            var obj = AppScoresRequestDTO.CreateBattleSessionRequest(userId, date, date, score, date, integration);
            var json = obj.ToJson();

            Assert.That(json.Contains(userId.ToString()));
            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));
            Assert.That(json.Contains(integration.id));

            obj = AppScoresRequestDTO.CreateModuleSessionRequest(date, score, date, integration);
            json = obj.ToJson();

            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));
            Assert.That(json.Contains(integration.id));

            obj = AppScoresRequestDTO.CreateDailyChallengeRequest(userId, date, date, score, date, integration);
            json = obj.ToJson();
            Assert.That(json.Contains(userId.ToString()));
            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));
            Assert.That(json.Contains(integration.id));

            obj = AppScoresRequestDTO.CreateDirectPlayRequest(userId, date, date, score, date, integration);
            json = obj.ToJson();
            Assert.That(json.Contains(userId.ToString()));
            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));
            Assert.That(json.Contains(integration.id));
        }

        [Test]
        public void TestToJSONWithNull()
        {
            var userId = Guid.NewGuid();
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);

            var obj = AppScoresRequestDTO.CreateBattleSessionRequest(userId, date, date, score, null, null);
            var json = obj.ToJson();
            Assert.That(json.Contains(userId.ToString()));
            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));

            obj = AppScoresRequestDTO.CreateModuleSessionRequest(date, score, null, null);
            json = obj.ToJson();
            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));

            obj = AppScoresRequestDTO.CreateDailyChallengeRequest(userId, date, date, score, null, null);
            json = obj.ToJson();
            Assert.That(json.Contains(userId.ToString()));
            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));

            obj = AppScoresRequestDTO.CreateDirectPlayRequest(userId, date, date, score, null, null);
            json = obj.ToJson();
            Assert.That(json.Contains(userId.ToString()));
            Assert.That(json.Contains(date.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            Assert.That(json.Contains(score.ToString()));
        }
    }
}
