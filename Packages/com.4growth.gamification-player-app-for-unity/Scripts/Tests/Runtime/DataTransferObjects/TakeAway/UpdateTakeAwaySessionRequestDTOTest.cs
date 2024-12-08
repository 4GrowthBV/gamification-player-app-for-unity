using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.TakeAway;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class UpdateTakeAwaySessionRequestDTOTest
    {
        [Test]
        public void Test()
        {
            var dto = new UpdateTakeAwaySessionRequestDTO(new DateTime(2001, 1, 1),
                new DateTime(2001, 1, 1));

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(dto.data.Type, "take_away_session");

            Assert.NotNull(dto.data.attributes);

            Assert.AreEqual(dto.data.attributes.StartedAt, new DateTime(2001, 1, 1));

            Assert.AreEqual(dto.data.attributes.EndedAt, new DateTime(2001, 1, 1));
        }
    }
}
