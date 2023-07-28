using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GetOrganisationResponseDTOTest : MonoBehaviour
    {
        [Test]
        public void Test()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetOrganisationResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(dto.data.Type, "organisation");

            Assert.NotNull(dto.data.attributes);

            Assert.That(dto.data.attributes.default_language != default);
            Assert.That(dto.data.attributes.introduction != default);
            Assert.That(dto.data.attributes.name != default);
            Assert.That(dto.data.attributes.primary_color != default);
            Assert.That(dto.data.attributes.subdomain != default);
            Assert.That(dto.data.attributes.webhook_url != default);
        }
    }
}
