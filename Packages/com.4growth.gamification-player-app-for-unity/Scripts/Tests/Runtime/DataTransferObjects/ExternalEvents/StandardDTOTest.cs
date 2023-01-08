using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class StandardDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new StandardDTO();

            obj.data.type = "pageView";

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new StandardDTO();
            
            obj.data.type = "pageView";

            var json = obj.ToJson();
            var newObj = json.FromJson<StandardDTO>();

            Assert.AreEqual(newObj.data.type, obj.data.type);
        }
    }
}
