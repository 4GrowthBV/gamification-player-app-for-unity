using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class QuitEventDTOTest
    {
        [Test]
        public void TestToJSON()
        {
            var obj = new QuitEventDTO();

            obj.data.type = "quitEvent";

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.type));
        }

        [Test]
        public void TestFromJSON()
        {
            var obj = new QuitEventDTO();
            
            obj.data.type = "quitEvent";

            var json = obj.ToJson();
            var newObj = json.FromJson<StandardDTO>();

            Assert.AreEqual(newObj.data.type, obj.data.type);
        }
    }
}
