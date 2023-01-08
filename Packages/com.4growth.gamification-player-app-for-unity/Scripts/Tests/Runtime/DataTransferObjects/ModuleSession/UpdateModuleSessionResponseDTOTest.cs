using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ModuleSession;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class UpdateModuleSessionResponseDTOTest
    {
        private readonly string mockServer = "https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676/module-sessions/497f6eca-6276-4993-bfeb-53cbbbba6f08";

        [UnityTest]
        public IEnumerator TestDTO()
        {
            return GamificationPlayerMockEndPoints.GetMockDTO(mockServer, (dto) =>
            {
                var obj = dto.FromJson<UpdateModuleSessionResponseDTO>();

                Assert.NotNull(obj);

                Assert.NotNull(obj.data);

                Assert.That(!string.IsNullOrEmpty(obj.data.id));

                Assert.AreEqual(obj.data.Type, "challenge_session");

                Assert.NotNull(obj.data.attributes);

                Assert.NotNull(obj.data.relationships);

                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.started_at));
                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.ended_at));
                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.completed_at));

                Assert.That(obj.data.attributes.StartedAt != default);
                Assert.That(obj.data.attributes.EndedAt != default);
                Assert.That(obj.data.attributes.CompletedAt != default);

                Assert.AreEqual(obj.data.relationships.user.data.type, "user");
                Assert.That(!string.IsNullOrEmpty(obj.data.relationships.user.data.id));

                Assert.AreEqual(obj.data.relationships.challenge.data.type, "challenge");
                Assert.That(!string.IsNullOrEmpty(obj.data.relationships.challenge.data.id));
            });
        }
    }
}
