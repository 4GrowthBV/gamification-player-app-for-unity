using GamificationPlayer.DTO.ModuleSession;
using NUnit.Framework;

namespace GamificationPlayer.Tests
{
    public class UpdateModuleSessionResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<UpdateModuleSessionResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));

            Assert.AreEqual(dto.data.Type, "module_session");

            Assert.NotNull(dto.data.attributes);

            Assert.NotNull(dto.data.relationships);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.started_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.ended_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.completed_at));

            Assert.That(dto.data.attributes.StartedAt != default);
            Assert.That(dto.data.attributes.EndedAt != default);
            Assert.That(dto.data.attributes.CompletedAt != default);

            Assert.AreEqual(dto.data.relationships.module.data.type, "module");
            Assert.That(!string.IsNullOrEmpty(dto.data.relationships.module.data.id));

            Assert.AreEqual(dto.data.relationships.challenge_session.data.type, "challenge_session");
            Assert.That(!string.IsNullOrEmpty(dto.data.relationships.challenge_session.data.id));
        }
    }
}
