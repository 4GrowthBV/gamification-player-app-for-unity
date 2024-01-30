using GamificationPlayer.DTO.MicroGame;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class GetMicroGameResponseDTOTest
    {
        [Test]
        public void Test()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetMicroGameResponseDTO>(out var dto);
        
            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));

            Assert.AreEqual(dto.data.Type, "micro_game");

            Assert.NotNull(dto.data.attributes);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.name));

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.description));

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.identifier));

            Assert.NotNull(dto.data.attributes.star_thresholds);

            Assert.That(dto.data.attributes.star_thresholds.Length > 0);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.web_gl_location));

            Assert.NotNull(dto.data.attributes.extra_data);            
        }
    }
}
