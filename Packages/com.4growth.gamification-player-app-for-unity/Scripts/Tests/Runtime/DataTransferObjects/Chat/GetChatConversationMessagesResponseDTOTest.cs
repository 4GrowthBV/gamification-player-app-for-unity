using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class GetChatConversationMessagesResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetChatConversationMessagesResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);
            Assert.That(dto.data.Count > 0);

            var firstMessage = dto.data[0];
            Assert.NotNull(firstMessage);

            Assert.That(!string.IsNullOrEmpty(firstMessage.id));
            Assert.AreEqual(firstMessage.Type, "chat_conversation_message");

            Assert.NotNull(firstMessage.attributes);

            Assert.That(!string.IsNullOrEmpty(firstMessage.attributes.role));
            Assert.That(!string.IsNullOrEmpty(firstMessage.attributes.message));
            Assert.That(!string.IsNullOrEmpty(firstMessage.attributes.created_at));
            Assert.That(!string.IsNullOrEmpty(firstMessage.attributes.updated_at));

            Assert.That(firstMessage.attributes.CreatedAt != default);
            Assert.That(firstMessage.attributes.UpdatedAt != default);

            Assert.NotNull(dto.included);
            Assert.NotNull(dto.links);
            Assert.NotNull(dto.meta);

            Assert.That(dto.meta.total >= 0);
            Assert.That(dto.meta.current_page >= 1);
        }
    }
}