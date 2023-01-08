using System;
using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class ModuleSessionStartedDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new ModuleSessionStartedDTO();

            obj.data.attributes.organisation_id = Guid.NewGuid().ToString();
            obj.data.attributes.user_id = Guid.NewGuid().ToString();
            obj.data.attributes.campaign_id = Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            obj.data.attributes.module_id = Guid.NewGuid().ToString();
            obj.data.attributes.module_session_id = Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.organisation_id));
            Assert.That(json.Contains(obj.data.attributes.user_id));
            Assert.That(json.Contains(obj.data.attributes.campaign_id));
            Assert.That(json.Contains(obj.data.attributes.challenge_id));
            Assert.That(json.Contains(obj.data.attributes.challenge_session_id));
            Assert.That(json.Contains(obj.data.attributes.module_id));
            Assert.That(json.Contains(obj.data.attributes.module_session_id));

            Assert.That(json.Contains(obj.data.type));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new ModuleSessionStartedDTO();

            obj.data.attributes.organisation_id = Guid.NewGuid().ToString();
            obj.data.attributes.user_id = Guid.NewGuid().ToString();
            obj.data.attributes.campaign_id = Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            obj.data.attributes.module_id = Guid.NewGuid().ToString();
            obj.data.attributes.module_session_id = Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            var json = obj.ToJson();
            var newObj = json.FromJson<ModuleSessionStartedDTO>();

            Assert.AreEqual(newObj.data.attributes.organisation_id, obj.data.attributes.organisation_id);
            Assert.AreEqual(newObj.data.attributes.user_id, obj.data.attributes.user_id);
            Assert.AreEqual(newObj.data.attributes.campaign_id, obj.data.attributes.campaign_id);
            Assert.AreEqual(newObj.data.attributes.challenge_id, obj.data.attributes.challenge_id);
            Assert.AreEqual(newObj.data.attributes.challenge_session_id, obj.data.attributes.challenge_session_id);
            Assert.AreEqual(newObj.data.attributes.module_id, obj.data.attributes.module_id);
            Assert.AreEqual(newObj.data.attributes.module_session_id, obj.data.attributes.module_session_id);

            Assert.AreEqual(newObj.data.type, obj.data.type);
        }
    }
}
