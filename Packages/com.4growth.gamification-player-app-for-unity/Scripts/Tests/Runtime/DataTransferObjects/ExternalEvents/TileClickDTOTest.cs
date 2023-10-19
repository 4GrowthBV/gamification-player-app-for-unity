using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class TileClickDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new TileClickDTO();

            obj.data.type = "tileClick";
            obj.data.attributes.identifier = "identifier";

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.identifier));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new TileClickDTO();
            
            obj.data.type = "tileClick";
            obj.data.attributes.identifier = "identifier";

            var json = obj.ToJson();
            var newObj = json.FromJson<TileClickDTO>();

            Assert.AreEqual(newObj.data.type, obj.data.type);
            Assert.AreEqual(newObj.data.attributes.identifier, obj.data.attributes.identifier);
        }
    }
}
