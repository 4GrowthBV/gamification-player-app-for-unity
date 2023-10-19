using GamificationPlayer.DTO.LoginToken;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class GetLoginTokenResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetLoginTokenResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(dto.data.Type, "login_token");

            Assert.NotNull(dto.data.attributes);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.expired_at));

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.token));
        }
    }
}
