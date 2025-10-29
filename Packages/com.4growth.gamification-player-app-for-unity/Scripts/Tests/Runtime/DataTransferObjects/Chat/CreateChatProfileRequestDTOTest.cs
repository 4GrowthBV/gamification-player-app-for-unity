using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class CreateChatProfileRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var profile = "This is a test chat profile";
            var chatConversationId = Guid.NewGuid();

            var obj = CreateChatProfileRequestDTO.Create(profile, chatConversationId);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_profile", obj.Data.Type);
            Assert.NotNull(obj.Data.Attributes);
            Assert.NotNull(obj.Data.Relationships);

            Assert.AreEqual(profile, obj.Data.Attributes.Profile);

            Assert.AreEqual("chat_conversation", obj.Data.Relationships.ChatConversation.Data.Type);
            Assert.AreEqual(chatConversationId.ToString(), obj.Data.Relationships.ChatConversation.Data.Id);
        }

        [Test]
        public void TestToJSON()
        {
            var profile = "Test profile content";
            var chatConversationId = Guid.NewGuid();

            var obj = CreateChatProfileRequestDTO.Create(profile, chatConversationId);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(profile));
            Assert.That(json.Contains(chatConversationId.ToString()));
            Assert.That(json.Contains("chat_conversation"));
        }

        [Test]
        public void TestFromJSON()
        {
            var profile = "Test chat profile";
            var chatConversationId = Guid.NewGuid();

            var obj = CreateChatProfileRequestDTO.Create(profile, chatConversationId);

            var json = obj.ToJson();
            var newObj = json.FromJson<CreateChatProfileRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Attributes.Profile == obj.Data.Attributes.Profile);
            Assert.That(newObj.Data.Relationships.ChatConversation.Data.Id == obj.Data.Relationships.ChatConversation.Data.Id);
        }
    }
}