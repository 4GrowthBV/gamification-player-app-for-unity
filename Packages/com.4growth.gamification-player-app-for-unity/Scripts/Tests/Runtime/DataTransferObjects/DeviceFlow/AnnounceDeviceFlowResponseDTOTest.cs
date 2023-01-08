using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.AnnounceDeviceFlow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class AnnounceDeviceFlowResponseDTOTest
    {
        private readonly string mockServer = "https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676/device-login"; //"https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676/device-login/497f6eca-6276-4993-bfeb-53cbbbba6f08";

        [UnityTest]
        public IEnumerator TestDTO()
        {
            var dto = new AnnounceDeviceFlowRequestDTO();
            var data = dto.ToJson();
            return GamificationPlayerMockEndPoints.GetMockDTOWithPost(mockServer, data, (dto) =>
            {
                var obj = dto.FromJson<AnnounceDeviceFlowResponseDTO>();

                Assert.NotNull(obj);

                Assert.NotNull(obj.data);

                Assert.That(!string.IsNullOrEmpty(obj.data.id));

                Assert.AreEqual(obj.data.Type, "device_login");

                Assert.NotNull(obj.data.attributes);

                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.expired_at));

                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.user_id));

                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.organisation_id));

                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.url));
            });
        }
    }
}
