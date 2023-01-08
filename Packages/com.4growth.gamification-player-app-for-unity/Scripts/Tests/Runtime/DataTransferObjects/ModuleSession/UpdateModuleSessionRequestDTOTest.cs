using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ModuleSession;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class UpdateModuleSessionRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var score = 100;
            var obj = new UpdateModuleSessionRequestDTO(new System.DateTime(2000, 1, 1), score, new System.DateTime(2001, 1, 1));

            Assert.AreEqual("module_session", obj.data.Type);
            Assert.AreEqual(new System.DateTime(2000, 1, 1), obj.data.attributes.EndedAt);
            Assert.AreEqual(new System.DateTime(2001, 1, 1), obj.data.attributes.CompletedAt);
            Assert.AreEqual(score, obj.data.attributes.score);
        }

        [Test]
        public void TestNullConstructor()
        {
            var score = 100;
            var obj = new UpdateModuleSessionRequestDTO(new System.DateTime(2000, 1, 1), score, null);

            Assert.AreEqual("module_session", obj.data.Type);
            Assert.AreEqual(new System.DateTime(2000, 1, 1), obj.data.attributes.EndedAt);
            Assert.AreEqual(null, obj.data.attributes.CompletedAt);
            Assert.AreEqual(score, obj.data.attributes.score);
        }

        [Test]
        public void TestToJSON()
        {
            var score = 777;
            var obj = new UpdateModuleSessionRequestDTO(new System.DateTime(2000, 1, 1), score, new System.DateTime(2001, 1, 1));
        
            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.completed_at));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
        }

        [Test]
        public void TestToJSONWithNull()
        {
            var score = 777;
            var obj = new UpdateModuleSessionRequestDTO(new System.DateTime(2000, 1, 1), score, null);
        
            var json = obj.ToJson();

            Debug.Log(json);

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains("null"));
            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
        }

        [Test]
        public void TestFromJSON()
        {
            var score = 777;
            var obj = new UpdateModuleSessionRequestDTO(new System.DateTime(2000, 1, 1), score, new System.DateTime(2001, 1, 1));

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateModuleSessionRequestDTO>();

            Assert.AreEqual(newObj.data.Type, obj.data.Type);
            Assert.AreEqual(newObj.data.attributes.CompletedAt, obj.data.attributes.CompletedAt);
            Assert.AreEqual(newObj.data.attributes.EndedAt, obj.data.attributes.EndedAt);
            Assert.AreEqual(newObj.data.attributes.score, obj.data.attributes.score);
        }

        [Test]
        public void TestFromJSONWithNull()
        {
            var score = 777;
            var obj = new UpdateModuleSessionRequestDTO(new System.DateTime(2000, 1, 1), score, null);

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateModuleSessionRequestDTO>();

            Assert.AreEqual(newObj.data.Type, obj.data.Type);
            Assert.AreEqual(newObj.data.attributes.CompletedAt, obj.data.attributes.CompletedAt);
            Assert.AreEqual(newObj.data.attributes.EndedAt, obj.data.attributes.EndedAt);
            Assert.AreEqual(newObj.data.attributes.score, obj.data.attributes.score);
        }
    }
}
