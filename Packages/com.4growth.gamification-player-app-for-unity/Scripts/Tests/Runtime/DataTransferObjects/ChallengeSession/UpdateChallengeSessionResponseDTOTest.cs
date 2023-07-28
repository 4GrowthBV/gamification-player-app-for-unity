using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.ChallengeSession;
using UnityEngine.Networking;

namespace GamificationPlayer.Tests
{
    public class UpdateChallengeSessionResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<UpdateChallengeSessionResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));

            Assert.AreEqual(dto.data.Type, "challenge_session");

            Assert.NotNull(dto.data.attributes);

            Assert.NotNull(dto.data.relationships);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.started_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.ended_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.completed_at));

            Assert.That(dto.data.attributes.started_at != default);
            Assert.That(dto.data.attributes.ended_at != default);
            Assert.That(dto.data.attributes.completed_at != default);

            Assert.AreEqual(dto.data.relationships.user.data.type, "user");
            Assert.That(!string.IsNullOrEmpty(dto.data.relationships.user.data.id));

            Assert.AreEqual(dto.data.relationships.challenge.data.type, "challenge");
            Assert.That(!string.IsNullOrEmpty(dto.data.relationships.challenge.data.id));
        }
    }
}
