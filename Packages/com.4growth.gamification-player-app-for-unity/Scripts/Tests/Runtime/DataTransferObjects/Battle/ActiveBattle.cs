using GamificationPlayer.DTO.Battle;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class ActiveBattleDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<ActiveBattleDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));

            Assert.AreEqual(dto.data.Type, "battle");

            Assert.NotNull(dto.data.attributes);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.name));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.game_id));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.micro_game_id));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.available_from));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.available_till));

            Assert.That(dto.data.attributes.name != default);
            Assert.That(dto.data.attributes.game_id != default);
            Assert.That(dto.data.attributes.micro_game_id != default);
            Assert.That(dto.data.attributes.available_from != default);
            Assert.That(dto.data.attributes.available_till != default);
        }
    }
}
