using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class UpdateChatConversationMessageRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var role = "user";
            var message = "Updated message content";
            var chatConversationId = Guid.NewGuid();

            var obj = UpdateChatConversationMessageRequestDTO.Create(role, message, chatConversationId);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_conversation_message", obj.Data.Type);
            Assert.NotNull(obj.Data.Attributes);
            Assert.NotNull(obj.Data.Relationships);

            Assert.AreEqual(role, obj.Data.Attributes.Role);
            Assert.AreEqual(message, obj.Data.Attributes.Message);

            Assert.AreEqual("chat_conversation", obj.Data.Relationships.ChatConversation.Data.Type);
            Assert.AreEqual(chatConversationId.ToString(), obj.Data.Relationships.ChatConversation.Data.Id);
        }

        [Test]
        public void TestToJSON()
        {
            var role = "assistant";
            var message = "Updated test message";
            var chatConversationId = Guid.NewGuid();

            var obj = UpdateChatConversationMessageRequestDTO.Create(role, message, chatConversationId);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(role));
            Assert.That(json.Contains(message));
            Assert.That(json.Contains(chatConversationId.ToString()));
            Assert.That(json.Contains("chat_conversation"));
        }

        [Test]
        public void TestFromJSON()
        {
            var role = "user";
            var message = "Updated message";
            var chatConversationId = Guid.NewGuid();

            var obj = UpdateChatConversationMessageRequestDTO.Create(role, message, chatConversationId);

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateChatConversationMessageRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Attributes.Role == obj.Data.Attributes.Role);
            Assert.That(newObj.Data.Attributes.Message == obj.Data.Attributes.Message);
            Assert.That(newObj.Data.Relationships.ChatConversation.Data.Id == obj.Data.Relationships.ChatConversation.Data.Id);
        }
    }
}