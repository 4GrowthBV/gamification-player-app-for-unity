using System;
using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class PageViewDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new PageViewDTO();

            obj.data.attributes.organisation_id = Guid.NewGuid().ToString();
            obj.data.attributes.user_id = Guid.NewGuid().ToString();

            obj.data.type = "pageView";

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.organisation_id));
            Assert.That(json.Contains(obj.data.attributes.user_id));

            Assert.That(json.Contains(obj.data.type));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new PageViewDTO();

            obj.data.attributes.organisation_id = Guid.NewGuid().ToString();
            obj.data.attributes.user_id = Guid.NewGuid().ToString();

            obj.data.attributes.user_is_demo = false;
            obj.data.attributes.organisation_allow_upgrade_to_registered_user = true;
            obj.data.attributes.user_tags = new string[] { };

            obj.data.type = "pageView";

            var json = obj.ToJson();
            var newObj = json.FromJson<PageViewDTO>(false);

            Assert.AreEqual(newObj.data.attributes.organisation_id, obj.data.attributes.organisation_id);
            Assert.AreEqual(newObj.data.attributes.user_id, obj.data.attributes.user_id);

            Assert.AreEqual(newObj.data.attributes.user_is_demo, obj.data.attributes.user_is_demo);
            Assert.AreEqual(newObj.data.attributes.organisation_allow_upgrade_to_registered_user, obj.data.attributes.organisation_allow_upgrade_to_registered_user);

            Assert.AreEqual(newObj.data.type, obj.data.type);
        }
    }
}
