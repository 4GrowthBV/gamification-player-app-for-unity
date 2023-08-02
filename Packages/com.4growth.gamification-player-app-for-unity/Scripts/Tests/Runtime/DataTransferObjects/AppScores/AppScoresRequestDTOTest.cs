using NUnit.Framework;
using GamificationPlayer.DTO.AppScores;
using System;

namespace GamificationPlayer.Tests
{
    public class AppScoresRequestDTOTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestConstructor()
        {
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var score = 888;

            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), 
                new System.DateTime(2001, 1, 1),
                userId,
                organisationId,
                battleSessionId,
                score, 
                new System.DateTime(2002, 1, 1));

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(new System.DateTime(2000, 1, 1), obj.data.attributes.StartedAt);
            Assert.AreEqual(new System.DateTime(2001, 1, 1), obj.data.attributes.EndedAt);
            Assert.AreEqual(new System.DateTime(2002, 1, 1), obj.data.attributes.CompletedAt);
        }

        [Test]
        public void TestNullConstructor()
        {
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var score = 888;

            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), 
                new System.DateTime(2001, 1, 1),
                userId,
                organisationId,
                battleSessionId,
                score, 
                null);

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(userId.ToString(), obj.data.attributes.user_id);
            Assert.AreEqual(organisationId.ToString(), obj.data.attributes.organisation_id);
            Assert.AreEqual(battleSessionId.ToString(), obj.data.attributes.battle_session_id);
            Assert.AreEqual(new System.DateTime(2000, 1, 1), obj.data.attributes.StartedAt);
            Assert.AreEqual(new System.DateTime(2001, 1, 1), obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);
        }

        [Test]
        public void TestToJSON()
        {
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var score = 888;

            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), 
                new System.DateTime(2001, 1, 1),
                userId,
                organisationId,
                battleSessionId,
                score, 
                new System.DateTime(2002, 1, 1));

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.started_at));
            Assert.That(json.Contains(obj.data.attributes.completed_at));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
        }

        [Test]
        public void TestToJSONWithNull()
        {
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var score = 888;

            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), 
                new System.DateTime(2000, 1, 1),
                userId,
                organisationId,
                battleSessionId,
                score, 
                null);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains("null"));
        }

        [Test]
        public void TestFromJSON()
        {
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var score = 888;

            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), 
                new System.DateTime(2000, 1, 1),
                userId,
                organisationId,
                battleSessionId,
                score, 
                null);

            var json = obj.ToJson();
            var newObj = json.FromJson<AppScoresRequestDTO>();

            Assert.That(newObj.data.Type == obj.data.Type);
            Assert.That(newObj.data.attributes.score == obj.data.attributes.score);
            Assert.That(newObj.data.attributes.CompletedAt == obj.data.attributes.CompletedAt);
            Assert.That(newObj.data.attributes.EndedAt == obj.data.attributes.EndedAt);
        }

        [Test]
        public void TestFromJSONWithNull()
        {
            var userId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var battleSessionId = Guid.NewGuid();
            var score = 888;

            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), 
                new System.DateTime(2000, 1, 1),
                userId,
                organisationId,
                battleSessionId,
                score, 
                null);

            var json = obj.ToJson();
            var newObj = json.FromJson<AppScoresRequestDTO>();

            Assert.That(newObj.data.Type == obj.data.Type);
            Assert.That(newObj.data.attributes.score == obj.data.attributes.score);
            Assert.That(newObj.data.attributes.CompletedAt == obj.data.attributes.CompletedAt);
            Assert.That(newObj.data.attributes.EndedAt == obj.data.attributes.EndedAt);
        }
    }
}
