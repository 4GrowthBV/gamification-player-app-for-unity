using NUnit.Framework;
using GamificationPlayer.DTO.AppScores;

namespace GamificationPlayer.Tests
{
    public class AppScoresRequestDTOTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestConstructor()
        {
            var score = 888;
            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), score, new System.DateTime(2001, 1, 1));

            Assert.AreEqual(score, obj.data.attributes.score);
            Assert.AreEqual(new System.DateTime(2000, 1, 1), obj.data.attributes.EndedAt);
            Assert.AreEqual(new System.DateTime(2001, 1, 1), obj.data.attributes.CompletedAt);
        }

        [Test]
        public void TestNullConstructor()
        {
            var score = 888;

            var newObj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), score, null);

            Assert.AreEqual(score, newObj.data.attributes.score);
            Assert.AreEqual(new System.DateTime(2000, 1, 1), newObj.data.attributes.EndedAt);
            Assert.AreEqual(null, newObj.data.attributes.CompletedAt);
        }

        [Test]
        public void TestToJSON()
        {
            var score = 888;
            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), score, new System.DateTime(2001, 1, 1));

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.completed_at));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
        }

        [Test]
        public void TestToJSONWithNull()
        {
            var score = 888;
            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), score, null);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.data.attributes.score.ToString()));
            Assert.That(json.Contains(obj.data.type));
            Assert.That(json.Contains(obj.data.attributes.ended_at));
            Assert.That(json.Contains("null"));
        }

        [Test]
        public void TestFromJSON()
        {
            var score = 888;
            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), score, new System.DateTime(2000, 1, 1));

            var json = obj.ToJson();
            var newObj = json.FromJson<AppScoresRequestDTO>();

            Assert.That(newObj.data.Type == obj.data.Type);
            Assert.That(newObj.data.attributes.score == obj.data.attributes.score);
            Assert.That(newObj.data.attributes.CompletedAt == obj.data.attributes.CompletedAt);
            Assert.That(newObj.data.attributes.EndedAt == obj.data.attributes.EndedAt);
        }

        [Test]
        public void TestFromJSONWithNull()
        {
            var score = 888;
            var obj = new AppScoresRequestDTO(new System.DateTime(2000, 1, 1), score, null);

            var json = obj.ToJson();
            var newObj = json.FromJson<AppScoresRequestDTO>();

            Assert.That(newObj.data.Type == obj.data.Type);
            Assert.That(newObj.data.attributes.score == obj.data.attributes.score);
            Assert.That(newObj.data.attributes.CompletedAt == obj.data.attributes.CompletedAt);
            Assert.That(newObj.data.attributes.EndedAt == obj.data.attributes.EndedAt);
        }
    }
}
