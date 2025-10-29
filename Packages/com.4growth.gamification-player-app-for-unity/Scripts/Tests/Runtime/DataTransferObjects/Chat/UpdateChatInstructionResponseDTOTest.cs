using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class UpdateChatInstructionResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<UpdateChatInstructionResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));
            Assert.AreEqual(dto.data.Type, "chat_instruction");

            Assert.NotNull(dto.data.attributes);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.identifier));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.instruction));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.created_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.updated_at));

            Assert.That(dto.data.attributes.CreatedAt != default);
            Assert.That(dto.data.attributes.UpdatedAt != default);

            Assert.NotNull(dto.included);
        }
    }
}