using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class CreateChatPredefinedMessageResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<CreateChatPredefinedMessageResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));
            Assert.AreEqual(dto.data.Type, "chat_predefined_message");

            Assert.NotNull(dto.data.attributes);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.identifier));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.content));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.button_name));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.created_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.updated_at));

            Assert.NotNull(dto.data.attributes.buttons);
            Assert.That(dto.data.attributes.buttons.Length > 0);

            Assert.That(dto.data.attributes.CreatedAt != default);
            Assert.That(dto.data.attributes.UpdatedAt != default);

            Assert.NotNull(dto.included);
        }
    }
}