using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class GetUserDTOTest : MonoBehaviour
    {
        [Test]
        public void Test()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetUserResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(dto.data.Type, "user");

            Assert.NotNull(dto.data.attributes);

            Assert.That(dto.data.attributes.name != default);
            Assert.That(dto.data.attributes.email != default);
            Assert.That(dto.data.attributes.avatar != default);

            Assert.NotNull(dto.included);
            Assert.That(dto.included.Length > 0);
            Assert.That(dto.included[0].attributes.name != default);
        }
    }
}
