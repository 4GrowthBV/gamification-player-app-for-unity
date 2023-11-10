using NUnit.Framework;
using GamificationPlayer.DTO.AppScores;
using System;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class AppScoresRequestDTOTest
    {
        private AppScoresRequestDTO GetAppScoresRequestBattleDTO(DateTime date, 
            int score, 
            Guid battleSessionId, 
            bool hasCompleted = true)
        {
            return new AppScoresRequestDTO(date, 
                score,
                Guid.Empty,
                battleSessionId,
                Guid.Empty,
                Guid.Empty,
                string.Empty,
                hasCompleted ? date : null);
        }

        private AppScoresRequestDTO GetAppScoresRequestModuleDTO(DateTime date, 
            int score, 
            Guid moduleSessionId, 
            bool hasCompleted = true)
        {           
            return new AppScoresRequestDTO(date, 
                score,
                moduleSessionId,
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                string.Empty,
                hasCompleted ? date : null);
        }

        private AppScoresRequestDTO GetAppScoresRequestMicroGameDTO(DateTime date, 
            int score, 
            Guid userId,
            Guid organisationId,
            string microGameId, 
            bool hasCompleted = true)
        {
            return new AppScoresRequestDTO(date, 
                score,
                Guid.Empty,
                Guid.Empty,
                userId,
                organisationId,
                microGameId,
                hasCompleted ? date : null);
        }

        [Test]
        public void TestConstructor()
        {
            var moduleSessionId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var microGameId = "PUZ72";
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);

            var obj = GetAppScoresRequestBattleDTO(date, score, battleSessionId);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);

            obj = GetAppScoresRequestModuleDTO(date, score, moduleSessionId);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(moduleSessionId.ToString(), obj.data.attributes.module_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(date, obj.data.attributes.CompletedAt);

            obj = GetAppScoresRequestMicroGameDTO(date, score, userId, organisationId, microGameId);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(microGameId, obj.data.attributes.micro_game_id);
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
            var microGameId = "PUZ72";
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);

            var obj = GetAppScoresRequestBattleDTO(date, score, battleSessionId, false);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);

            obj = GetAppScoresRequestModuleDTO(date, score, moduleSessionId, false);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(moduleSessionId.ToString(), obj.data.attributes.module_session_id);
            Assert.AreEqual(date, obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);

            obj = GetAppScoresRequestMicroGameDTO(date, score, userId, organisationId, microGameId, false);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(microGameId, obj.data.attributes.micro_game_id);
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
            var microGameId = "PUZ72";
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);

            var obj = GetAppScoresRequestBattleDTO(date, score, battleSessionId);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.battle_session_id));

            obj = GetAppScoresRequestModuleDTO(date, score, moduleSessionId);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.module_session_id));

            obj = GetAppScoresRequestMicroGameDTO(date, score, userId, organisationId, microGameId);

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
            var microGameId = "PUZ72";
            var score = 888;
            var date = new System.DateTime(2001, 1, 1);

            var obj = GetAppScoresRequestBattleDTO(date, score, battleSessionId, false);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.battle_session_id));
            Assert.That(json.Contains("null"));

            obj = GetAppScoresRequestModuleDTO(date, score, moduleSessionId, false);

            json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.module_session_id));
            Assert.That(json.Contains("null"));

            obj = GetAppScoresRequestMicroGameDTO(date, score, userId, organisationId, microGameId, false);

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
