using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;

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

            var json = obj.ToJson();

            Assert.That(json.Contains("moduleData"));
            Assert.That(json.Contains("testKey"));
            Assert.That(json.Contains("testValue"));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new MicroGamePayload();

            obj.micro_game.extra_data = new System.Collections.Generic.Dictionary<string, string>()
            {
                { "testKey", "testValue" } 
            };

            var json = obj.ToJson();
            var newObj = json.FromJson<MicroGamePayload>();

            Assert.AreEqual(newObj.Type, "moduleData");
            Assert.AreEqual(newObj.micro_game.extra_data["testKey"], "testValue");
        }
    }
}
