using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class JSONWebTokenPayloadTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new MicroGamePayload();

            obj.micro_game.extra_data = new System.Collections.Generic.Dictionary<string, string>()
            {
                { "testKey", "testValue" } 
            };

            obj.integration = new MicroGamePayload.Integration()
            {
                id = "integrationId",
                context = new System.Collections.Generic.Dictionary<string, string>()
                {
                    { "contextkey", "contextValue" }
                }
            };

            var json = obj.ToJson();

            Assert.That(json.Contains("moduleData"));
            Assert.That(json.Contains("testKey"));
            Assert.That(json.Contains("testValue"));

            Assert.That(json.Contains("integrationId"));
            Assert.That(json.Contains("contextkey"));
            Assert.That(json.Contains("contextValue"));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new MicroGamePayload();

            obj.micro_game.extra_data = new System.Collections.Generic.Dictionary<string, string>()
            {
                { "testKey", "testValue" } 
            };

            obj.integration = new MicroGamePayload.Integration()
            {
                id = "integrationId",
                context = new System.Collections.Generic.Dictionary<string, string>()
                {
                    { "contextkey", "contextValue" }
                }
            };

            obj.player.user_tags = new string[] {};


            var json = obj.ToJson();

            Debug.Log(json);

            var newObj = json.FromJson<MicroGamePayload>(false);

            Assert.AreEqual(newObj.Type, "moduleData");
            Assert.AreEqual(newObj.micro_game.extra_data["testKey"], "testValue");

            Assert.AreEqual(newObj.integration.id, "integrationId");
            Assert.AreEqual(newObj.integration.context["contextkey"], "contextValue");
        }
    }
}
