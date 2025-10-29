using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class CreateChatConversationRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var organisationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatConversationRequestDTO.Create(organisationId, userId, microGameId);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_conversation", obj.Data.Type);
            Assert.NotNull(obj.Data.Relationships);

            Assert.AreEqual("organisation", obj.Data.Relationships.Organisation.Data.Type);
            Assert.AreEqual(organisationId.ToString(), obj.Data.Relationships.Organisation.Data.Id);

            Assert.AreEqual("user", obj.Data.Relationships.User.Data.Type);
            Assert.AreEqual(userId.ToString(), obj.Data.Relationships.User.Data.Id);

            Assert.AreEqual("micro_game", obj.Data.Relationships.MicroGame.Data.Type);
            Assert.AreEqual(microGameId.ToString(), obj.Data.Relationships.MicroGame.Data.Id);
        }

        [Test]
        public void TestToJSON()
        {
            var organisationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatConversationRequestDTO.Create(organisationId, userId, microGameId);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(organisationId.ToString()));
            Assert.That(json.Contains(userId.ToString()));
            Assert.That(json.Contains(microGameId.ToString()));
            Assert.That(json.Contains("organisation"));
            Assert.That(json.Contains("user"));
            Assert.That(json.Contains("micro_game"));
        }

        [Test]
        public void TestFromJSON()
        {
            var organisationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatConversationRequestDTO.Create(organisationId, userId, microGameId);

            var json = obj.ToJson();
            var newObj = json.FromJson<CreateChatConversationRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Relationships.Organisation.Data.Id == obj.Data.Relationships.Organisation.Data.Id);
            Assert.That(newObj.Data.Relationships.User.Data.Id == obj.Data.Relationships.User.Data.Id);
            Assert.That(newObj.Data.Relationships.MicroGame.Data.Id == obj.Data.Relationships.MicroGame.Data.Id);
        }
    }
}