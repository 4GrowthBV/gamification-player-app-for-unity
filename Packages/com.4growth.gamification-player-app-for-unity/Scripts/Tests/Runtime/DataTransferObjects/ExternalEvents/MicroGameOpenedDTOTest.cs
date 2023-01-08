using System;
using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class MicroGameOpenedDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new MicroGameOpenedDTO();

            obj.data.type = "microGameOpened";
            obj.data.attributes.identifier = Guid.NewGuid().ToString();

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.identifier));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new MicroGameOpenedDTO();
            
            obj.data.type = "microGameOpened";
            obj.data.attributes.identifier = Guid.NewGuid().ToString();

            var json = obj.ToJson();
            var newObj = json.FromJson<FitnessContentOpenedDTO>();

            Assert.AreEqual(newObj.data.type, obj.data.type);
            Assert.AreEqual(newObj.data.attributes.identifier, obj.data.attributes.identifier);
        }
    }
}
