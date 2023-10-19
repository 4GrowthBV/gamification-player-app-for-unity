using System.Linq;
using GamificationPlayer.DTO.Battle;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class GetOpenBattleInvitationsForUserDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetOpenBattleInvitationsForUserDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(dto.data.Count() == 1);
        }
    }
}
