using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.GetDeviceFlow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GetDeviceFlowResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetDeviceFlowResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));

            Assert.AreEqual(dto.data.Type, "device_login");

            Assert.NotNull(dto.data.attributes);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.expired_at));

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.user_id));

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.organisation_id));

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.url));

        }
    }
}
