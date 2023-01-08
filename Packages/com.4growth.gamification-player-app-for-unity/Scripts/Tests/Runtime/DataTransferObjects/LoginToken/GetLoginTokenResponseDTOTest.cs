using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.LoginToken;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GetLoginTokenResponseDTOTest
    {
        private readonly string mockServer = "https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676/organisations/497f6eca-6276-4993-bfeb-53cbbbba6f08/users/497f6eca-6276-4993-bfeb-53cbbbba6f08/login-token";

        [UnityTest]
        public IEnumerator TestDTO()
        {
            return GamificationPlayerMockEndPoints.GetMockDTO(mockServer, (dto) =>
            {
                var obj = dto.FromJson<GetLoginTokenResponseDTO>();

                Assert.NotNull(obj);

                Assert.NotNull(obj.data);

                Assert.AreEqual(obj.data.Type, "login_token");

                Assert.NotNull(obj.data.attributes);

                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.expired_at));

                Assert.That(!string.IsNullOrEmpty(obj.data.attributes.token));
            });
        }
    }
}
