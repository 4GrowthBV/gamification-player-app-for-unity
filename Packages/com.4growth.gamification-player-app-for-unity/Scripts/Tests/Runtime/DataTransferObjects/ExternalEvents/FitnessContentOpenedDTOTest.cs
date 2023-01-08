using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class FitnessContentOpenedDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new FitnessContentOpenedDTO();

            obj.data.type = "fitnessContentOpened";
            obj.data.attributes.identifier = Guid.NewGuid().ToString();

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.identifier));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new FitnessContentOpenedDTO();
            
            obj.data.type = "fitnessContentOpened";
            obj.data.attributes.identifier = Guid.NewGuid().ToString();

            var json = obj.ToJson();
            var newObj = json.FromJson<FitnessContentOpenedDTO>();

            Assert.AreEqual(newObj.data.type, obj.data.type);
            Assert.AreEqual(newObj.data.attributes.identifier, obj.data.attributes.identifier);
        }
    }
}
