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
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<TimeResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(dto.data.Type, "time");

            Assert.NotNull(dto.data.attributes);

            Assert.NotNull(dto.data.attributes.now);

            Assert.That(DateTime.TryParse(dto.data.attributes.now, out _));
        }
    }
}
