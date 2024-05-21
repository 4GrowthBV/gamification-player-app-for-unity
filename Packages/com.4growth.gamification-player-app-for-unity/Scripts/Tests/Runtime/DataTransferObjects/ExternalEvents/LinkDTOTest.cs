using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class LinkDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new LinkDTO();

            obj.data.type = "link";
            obj.data.attributes.link = "link";

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.link));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new LinkDTO();
            
            obj.data.type = "link";
            obj.data.attributes.link = "link";

            var json = obj.ToJson();
            var newObj = json.FromJson<LinkDTO>();

            Assert.AreEqual(newObj.data.type, obj.data.type);
            Assert.AreEqual(newObj.data.attributes.link, obj.data.attributes.link);
        }
    }
}
