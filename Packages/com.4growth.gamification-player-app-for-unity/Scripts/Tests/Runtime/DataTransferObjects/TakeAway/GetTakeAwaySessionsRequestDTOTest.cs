using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.TakeAway;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GetTakeAwaySessionsRequestDTOTest
    {
        [Test]
        public void Test()
        {
            var dto = new GetTakeAwaySessionsRequestDTO(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            Assert.NotNull(dto);

            Assert.NotNull(dto.micro_game_id);
            Assert.NotNull(dto.user_id);
            Assert.NotNull(dto.organisation_id);
            Assert.NotNull(dto.module_session_id);
        }
    }
}
