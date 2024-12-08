using GamificationPlayer.DTO.TakeAway;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class TakeAwaySessionResponseDTOTest
    {
        [Test]
        public void Test()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<TakeAwaySessionResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(dto.data.Type, "take_away_session");

            Assert.NotNull(dto.data.attributes);

            Assert.NotNull(dto.data.attributes.StartedAt);

            Assert.NotNull(dto.data.attributes.EndedAt);

            Assert.NotNull(dto.data.relationships);

            Assert.NotNull(dto.data.relationships.micro_game);

            Assert.NotNull(dto.data.relationships.user);

            Assert.NotNull(dto.data.relationships.organisation);

            Assert.NotNull(dto.data.relationships.module_session);

            Assert.NotNull(dto.data.relationships.micro_game.data.id);

            Assert.NotNull(dto.data.relationships.user.data.id);

            Assert.NotNull(dto.data.relationships.organisation.data.id);

            Assert.NotNull(dto.data.relationships.module_session.data.id);
        }
    }
}
