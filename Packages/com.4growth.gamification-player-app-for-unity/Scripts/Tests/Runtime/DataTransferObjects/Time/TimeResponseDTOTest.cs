using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class TimeResponseDTOTest
    {
        private readonly string mockServer = "https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676/time";

        [UnityTest]
        public IEnumerator TestDTO()
        {
            return GamificationPlayerMockEndPoints.GetMockDTO(mockServer, (dto) =>
            {
                var obj = dto.FromJson<TimeResponseDTO>();

                Assert.NotNull(obj);

                Assert.NotNull(obj.data);

                Assert.AreEqual(obj.data.Type, "time");

                Assert.NotNull(obj.data.attributes);

                Assert.NotNull(obj.data.attributes.now);

                Assert.That(DateTime.TryParse(obj.data.attributes.now, out _));
            });
        }
    }
}
