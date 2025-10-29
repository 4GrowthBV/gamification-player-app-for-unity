using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class UpdateChatConversationMessageResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<UpdateChatConversationMessageResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data.id));
            Assert.AreEqual(dto.data.Type, "chat_conversation_message");

            Assert.NotNull(dto.data.attributes);

            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.role));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.message));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.created_at));
            Assert.That(!string.IsNullOrEmpty(dto.data.attributes.updated_at));

            Assert.That(dto.data.attributes.CreatedAt != default);
            Assert.That(dto.data.attributes.UpdatedAt != default);

            Assert.NotNull(dto.included);
        }
    }
}