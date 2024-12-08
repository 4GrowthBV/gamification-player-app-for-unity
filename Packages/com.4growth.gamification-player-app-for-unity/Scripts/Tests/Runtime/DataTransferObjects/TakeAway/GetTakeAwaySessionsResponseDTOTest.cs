using GamificationPlayer.DTO.TakeAway;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class GetTakeAwaySessionsResponseDTOTest
    {
        [Test]
        public void Test()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetTakeAwaySessionsResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(1, dto.data.Length);

            var data = dto.data[0];

            Assert.AreEqual("take_away_session", data.Type);

            Assert.NotNull(data.attributes);
        }
    }
}
