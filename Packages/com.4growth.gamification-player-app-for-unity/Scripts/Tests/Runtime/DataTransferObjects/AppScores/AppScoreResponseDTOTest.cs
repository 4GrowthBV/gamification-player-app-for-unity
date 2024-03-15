using GamificationPlayer.DTO.AppScores;
using GamificationPlayer.DTO.ModuleSession;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class AppScoresResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<AppScoresRespondDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));

            Assert.AreEqual(dto.data.Type, "app_score");

            Assert.NotNull(dto.data.attributes);
            Assert.NotNull(dto.data.links);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.started_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.ended_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.completed_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.links.show));

            Assert.That(dto.data.attributes.StartedAt != default);
            Assert.That(dto.data.attributes.EndedAt != default);
            Assert.That(dto.data.attributes.CompletedAt != default);
        }
    }
}
