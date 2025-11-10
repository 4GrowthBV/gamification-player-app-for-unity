using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class GetChatConversationsResponseDTOTest
    {
        [Test]
        public void TestDTO()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetChatConversationsResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);
            Assert.That(dto.data.Count > 0);

            var firstItem = dto.data[0];
            Assert.NotNull(firstItem);

            Assert.That(!string.IsNullOrEmpty(firstItem.id));
            Assert.AreEqual(firstItem.Type, "chat_conversation");

            Assert.NotNull(firstItem.attributes);

            Assert.That(!string.IsNullOrEmpty(firstItem.attributes.created_at));
            Assert.That(!string.IsNullOrEmpty(firstItem.attributes.updated_at));

            Assert.That(firstItem.attributes.CreatedAt != default);
            Assert.That(firstItem.attributes.UpdatedAt != default);

            Assert.NotNull(dto.included);
            Assert.NotNull(dto.links);
            Assert.NotNull(dto.meta);
        }
    }
}