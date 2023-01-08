using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.ChallengeSession;

namespace GamificationPlayer.Tests
{
    public class UpdateChallendeSessionRequestDTOTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestConstructor()
        {
            var obj = new UpdateChallendeSessionRequestDTO(new System.DateTime(2000, 1, 1), new System.DateTime(2001, 1, 1));

            Assert.AreEqual(new System.DateTime(2000, 1, 1), obj.data.attributes.EndedAt);
            Assert.AreEqual(new System.DateTime(2001, 1, 1), obj.data.attributes.CompletedAt);
        }

        [Test]
        public void TestNullConstructor()
        {
            var newObj = new UpdateChallendeSessionRequestDTO(new System.DateTime(2000, 1, 1), null);

            Assert.AreEqual(new System.DateTime(2000, 1, 1), newObj.data.attributes.EndedAt);
            Assert.AreEqual(null, newObj.data.attributes.CompletedAt);
        }

        [Test]
        public void TestToJSON()
        {
            var obj = new UpdateChallendeSessionRequestDTO(new System.DateTime(2000, 1, 1), new System.DateTime(2001, 1, 1));

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.completed_at));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
        }

        [Test]
        public void TestToJSONWithNull()
        {
            var obj = new UpdateChallendeSessionRequestDTO(new System.DateTime(2000, 1, 1), null);

            var json = obj.ToJson();

            Debug.Log(json);

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains("null"));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new UpdateChallendeSessionRequestDTO(new System.DateTime(2000, 1, 1), new System.DateTime(2001, 1, 1));

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateChallendeSessionRequestDTO>();

            Assert.That(newObj.data.Type == obj.data.Type);
            Assert.That(newObj.data.attributes.CompletedAt == obj.data.attributes.CompletedAt);
            Assert.That(newObj.data.attributes.EndedAt == obj.data.attributes.EndedAt);
        }

        [Test]
        public void TestFromJSONWithNull()
        {
            var obj = new UpdateChallendeSessionRequestDTO(new System.DateTime(2000, 1, 1), null);

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateChallendeSessionRequestDTO>();

            Assert.That(newObj.data.Type == obj.data.Type);
            Assert.That(newObj.data.attributes.CompletedAt == obj.data.attributes.CompletedAt);
            Assert.That(newObj.data.attributes.EndedAt == obj.data.attributes.EndedAt);
        }
    }
}
