using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.TakeAway;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class CreateTakeAwaySessionRequestDTOTest
    {
        [Test]
        public void Test()
        {
            var dto = new CreateTakeAwaySessionRequestDTO(new DateTime(2001, 1, 1),
                new DateTime(2001, 1, 1),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid());

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.AreEqual(dto.data.Type, "take_away_session");

            Assert.NotNull(dto.data.attributes);

            Assert.NotNull(dto.data.attributes.started_at);

            Assert.NotNull(dto.data.attributes.ended_at);

            Assert.NotNull(dto.data.relationships);

            Assert.NotNull(dto.data.relationships.micro_game);

            Assert.AreEqual(dto.data.relationships.micro_game.data.type, "micro_game");

            Assert.NotNull(dto.data.relationships.micro_game.data.id);

            Assert.NotNull(dto.data.relationships.user);

            Assert.AreEqual(dto.data.relationships.user.data.type, "user");

            Assert.NotNull(dto.data.relationships.user.data.id);

            Assert.NotNull(dto.data.relationships.organisation);

            Assert.AreEqual(dto.data.relationships.organisation.data.type, "organisation");

            Assert.NotNull(dto.data.relationships.organisation.data.id);

            Assert.NotNull(dto.data.relationships.module_session);

            Assert.AreEqual(dto.data.relationships.module_session.data.type, "module_session");

            Assert.NotNull(dto.data.relationships.module_session.data.id);
        }
    }
}
