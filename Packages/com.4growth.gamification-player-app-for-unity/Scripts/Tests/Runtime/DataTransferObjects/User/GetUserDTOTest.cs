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

            Assert.NotNull(dto.data.relationships);
            Assert.NotNull(dto.data.relationships.tags);
            Assert.NotNull(dto.data.relationships.tags.data);
            Assert.That(dto.data.relationships.tags.data.Length > 0);
            Assert.That(dto.data.relationships.tags.data[0].name != default);
        }
    }
}
