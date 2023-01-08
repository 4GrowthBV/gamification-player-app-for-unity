using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ModuleSession;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GetModuleSessionsResponseDTOTest
    {
        private readonly string mockServer = "https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676/module-sessions";

        [UnityTest]
        public IEnumerator TestDTO()
        {
            return GamificationPlayerMockEndPoints.GetMockDTO(mockServer, (dto) =>
            {
                var obj = dto.FromJson<GetModuleSessionsResponseDTO>();

                Assert.NotNull(obj);

                Assert.NotNull(obj.data);

                Assert.That(!string.IsNullOrEmpty(obj.data[0].id));

                Assert.AreEqual(obj.data[0].Type, "module_session");

                Assert.NotNull(obj.data[0].attributes);

                Assert.NotNull(obj.data[0].relationships);

                Assert.That(!string.IsNullOrEmpty(obj.data[0].attributes.started_at));
                Assert.That(!string.IsNullOrEmpty(obj.data[0].attributes.ended_at));
                Assert.That(!string.IsNullOrEmpty(obj.data[0].attributes.completed_at));

                Assert.That(obj.data[0].attributes.started_at != default);
                Assert.That(obj.data[0].attributes.ended_at != default);
                Assert.That(obj.data[0].attributes.completed_at != default);

                Assert.AreEqual(obj.data[0].relationships.challenge_session.data.type, "challenge_session");
                Assert.That(!string.IsNullOrEmpty(obj.data[0].relationships.challenge_session.data.id));

                Assert.AreEqual(obj.data[0].relationships.module.data.type, "module");
                Assert.That(!string.IsNullOrEmpty(obj.data[0].relationships.module.data.id));
            });
        }
    }
}
