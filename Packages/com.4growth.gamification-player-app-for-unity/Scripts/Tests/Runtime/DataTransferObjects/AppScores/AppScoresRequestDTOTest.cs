using NUnit.Framework;
using GamificationPlayer.DTO.AppScores;
using System;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class AppScoresRequestDTOTest
    {
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

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, organisationId, battleSessionId, date);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);

            obj = AppScoresRequestDTO.GetAppScoresModuleRequest(date, date, score, moduleSessionId, date);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(moduleSessionId.ToString(), obj.data.attributes.module_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);

            obj = AppScoresRequestDTO.GetAppScoresRequest(date, date, score, userId, organisationId, microGameId, date);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(microGameId.ToString(), obj.data.attributes.micro_game_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);
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

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, organisationId, battleSessionId, null);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);

            obj = AppScoresRequestDTO.GetAppScoresModuleRequest(date, date, score, moduleSessionId, null);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(moduleSessionId.ToString(), obj.data.attributes.module_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);

            obj = AppScoresRequestDTO.GetAppScoresRequest(date, date, score, userId, organisationId, microGameId, null);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(microGameId.ToString(), obj.data.attributes.micro_game_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);
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

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, organisationId, battleSessionId, date);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.battle_session_id));

            obj = AppScoresRequestDTO.GetAppScoresModuleRequest(date, date, score, moduleSessionId, date);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.module_session_id));

            obj = AppScoresRequestDTO.GetAppScoresRequest(date, date, score, userId, organisationId, microGameId, date);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.user_id));
            Assert.That(json.Contains(obj.data.attributes.organisation_id));
            Assert.That(json.Contains(obj.data.attributes.micro_game_id));
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

            var obj = AppScoresRequestDTO.GetAppScoresBattleRequest(date, date, score, userId, organisationId, battleSessionId, null);

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
