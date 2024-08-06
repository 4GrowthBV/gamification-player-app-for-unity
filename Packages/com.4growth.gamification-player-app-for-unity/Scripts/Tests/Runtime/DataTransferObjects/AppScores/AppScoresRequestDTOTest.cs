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
            var moduleSessionId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();
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

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, battleSessionId, date, integration);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);
            Assert.AreEqual(integration.id, obj.data.attributes.integration.id);
            Assert.AreEqual(integration.context, obj.data.attributes.integration.context);

            obj = AppScoresRequestDTO.GetAppScoresModuleRequest(date, date, score, moduleSessionId, date, integration);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(moduleSessionId.ToString(), obj.data.attributes.module_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);
            Assert.AreEqual(integration.id, obj.data.attributes.integration.id);
            Assert.AreEqual(integration.context, obj.data.attributes.integration.context);

            obj = AppScoresRequestDTO.GetAppScoresRequest(date, date, score, userId, organisationId, microGameId, date, integration);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(microGameId.ToString(), obj.data.attributes.micro_game_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);
            Assert.AreEqual(integration.id, obj.data.attributes.integration.id);
            Assert.AreEqual(integration.context, obj.data.attributes.integration.context);
        }

        [Test]
        public void TestNullConstructor()
        {
            var moduleSessionId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();
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

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, battleSessionId, null);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);
            Assert.AreEqual(null, obj.data.attributes.integration);

            obj = AppScoresRequestDTO.GetAppScoresModuleRequest(date, date, score, moduleSessionId, null);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(moduleSessionId.ToString(), obj.data.attributes.module_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);
            Assert.AreEqual(null, obj.data.attributes.integration);

            obj = AppScoresRequestDTO.GetAppScoresRequest(date, date, score, userId, organisationId, microGameId, null);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(microGameId.ToString(), obj.data.attributes.micro_game_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);
            Assert.AreEqual(null, obj.data.attributes.integration);
        }

        [Test]
        public void TestToJSON()
        {
            var moduleSessionId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();
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

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, battleSessionId, date, integration);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.battle_session_id));
            Assert.That(json.Contains(obj.data.attributes.integration.id));
            Assert.That(json.Contains(obj.data.attributes.integration.context["key"]));

            obj = AppScoresRequestDTO.GetAppScoresModuleRequest(date, date, score, moduleSessionId, date, integration);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.module_session_id));
            Assert.That(json.Contains(obj.data.attributes.integration.id));
            Assert.That(json.Contains(obj.data.attributes.integration.context["key"]));

            obj = AppScoresRequestDTO.GetAppScoresRequest(date, date, score, userId, organisationId, microGameId, date, integration);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.user_id));
            Assert.That(json.Contains(obj.data.attributes.organisation_id));
            Assert.That(json.Contains(obj.data.attributes.micro_game_id));
            Assert.That(json.Contains(obj.data.attributes.integration.id));
            Assert.That(json.Contains(obj.data.attributes.integration.context["key"]));

            Debug.Log(json);
        }

        [Test]
        public void TestToJSONWithNull()
        {
            var moduleSessionId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, battleSessionId, null);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.battle_session_id));
            Assert.That(json.Contains("null"));

            obj = AppScoresRequestDTO.GetAppScoresModuleRequest(date, date, score, moduleSessionId, null);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.module_session_id));
            Assert.That(json.Contains("null"));

            obj = AppScoresRequestDTO.GetAppScoresRequest(date, date, score, userId, organisationId, microGameId, null);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.user_id));
            Assert.That(json.Contains(obj.data.attributes.organisation_id));
            Assert.That(json.Contains(obj.data.attributes.micro_game_id));
            Assert.That(json.Contains("null"));
        }
    }
}
