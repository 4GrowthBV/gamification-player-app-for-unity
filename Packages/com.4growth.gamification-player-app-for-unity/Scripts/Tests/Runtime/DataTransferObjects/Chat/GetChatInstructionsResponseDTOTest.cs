using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class GetChatInstructionsResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetChatInstructionsResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);
            Assert.That(dto.data.Count > 0);

            var firstInstruction = dto.data[0];
            Assert.NotNull(firstInstruction);

            Assert.That(!string.IsNullOrEmpty(firstInstruction.id));
            Assert.AreEqual(firstInstruction.Type, "chat_instruction");

            Assert.NotNull(firstInstruction.attributes);

            Assert.That(!string.IsNullOrEmpty(firstInstruction.attributes.identifier));
            Assert.That(!string.IsNullOrEmpty(firstInstruction.attributes.instruction));
            Assert.That(!string.IsNullOrEmpty(firstInstruction.attributes.created_at));
            Assert.That(!string.IsNullOrEmpty(firstInstruction.attributes.updated_at));

            Assert.That(firstInstruction.attributes.CreatedAt != default);
            Assert.That(firstInstruction.attributes.UpdatedAt != default);

            Assert.NotNull(dto.included);
            Assert.NotNull(dto.links);
            Assert.NotNull(dto.meta);

            Assert.That(dto.meta.total >= 0);
            Assert.That(dto.meta.current_page >= 1);
        }
    }
}