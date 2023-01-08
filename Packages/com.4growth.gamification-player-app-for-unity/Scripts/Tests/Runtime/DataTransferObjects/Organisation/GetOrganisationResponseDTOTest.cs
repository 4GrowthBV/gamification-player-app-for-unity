using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GetOrganisationResponseDTOTest : MonoBehaviour
    {
        private readonly string mockServer = "https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676/organisations/497f6eca-6276-4993-bfeb-53cbbbba6f08";

        [UnityTest]
        public IEnumerator TestDTO()
        {
            return GamificationPlayerMockEndPoints.GetMockDTO(mockServer, (dto) =>
            {
                var obj = dto.FromJson<GetOrganisationResponseDTO>();

                Assert.NotNull(obj);

                Assert.NotNull(obj.data);

                Assert.AreEqual(obj.data.Type, "organisation");

                Assert.NotNull(obj.data.attributes);

                Assert.That(obj.data.attributes.default_language != default);
                Assert.That(obj.data.attributes.introduction != default);
                Assert.That(obj.data.attributes.name != default);
                Assert.That(obj.data.attributes.primary_color != default);
                Assert.That(obj.data.attributes.subdomain != default);
                Assert.That(obj.data.attributes.webhook_url != default);
            });
        }
    }
}
