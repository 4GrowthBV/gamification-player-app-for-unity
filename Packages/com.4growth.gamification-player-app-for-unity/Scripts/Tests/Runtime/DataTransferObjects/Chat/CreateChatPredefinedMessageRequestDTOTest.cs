using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class CreateChatPredefinedMessageRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var identifier = "greeting_message";
            var content = "Hello! How can I assist you today?";
            var buttons = new List<string> { "Help", "Support", "FAQ" };
            var buttonName = "Quick Actions";
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatPredefinedMessageRequestDTO.Create(identifier, content, buttons, buttonName, organisationId, microGameId);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_predefined_message", obj.Data.Type);
            Assert.NotNull(obj.Data.Attributes);
            Assert.NotNull(obj.Data.Relationships);

            Assert.AreEqual(identifier, obj.Data.Attributes.Identifier);
            Assert.AreEqual(content, obj.Data.Attributes.Content);
            Assert.AreEqual(buttons, obj.Data.Attributes.Buttons);
            Assert.AreEqual(buttonName, obj.Data.Attributes.ButtonName);

            Assert.AreEqual("organisation", obj.Data.Relationships.Organisation.Data.Type);
            Assert.AreEqual(organisationId.ToString(), obj.Data.Relationships.Organisation.Data.Id);

            Assert.AreEqual("micro_game", obj.Data.Relationships.MicroGame.Data.Type);
            Assert.AreEqual(microGameId.ToString(), obj.Data.Relationships.MicroGame.Data.Id);
        }

        [Test]
        public void TestToJSON()
        {
            var identifier = "test_message";
            var content = "Test message content";
            var buttons = new List<string> { "Option1", "Option2" };
            var buttonName = "Actions";
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatPredefinedMessageRequestDTO.Create(identifier, content, buttons, buttonName, organisationId, microGameId);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(identifier));
            Assert.That(json.Contains(content));
            Assert.That(json.Contains(buttonName));
            Assert.That(json.Contains("Option1"));
            Assert.That(json.Contains("Option2"));
            Assert.That(json.Contains(organisationId.ToString()));
            Assert.That(json.Contains(microGameId.ToString()));
        }

        [Test]
        public void TestFromJSON()
        {
            var identifier = "test_message";
            var content = "Test content";
            var buttons = new List<string> { "Button1", "Button2" };
            var buttonName = "Test Actions";
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatPredefinedMessageRequestDTO.Create(identifier, content, buttons, buttonName, organisationId, microGameId);

            var json = obj.ToJson();
            var newObj = json.FromJson<CreateChatPredefinedMessageRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Attributes.Identifier == obj.Data.Attributes.Identifier);
            Assert.That(newObj.Data.Attributes.Content == obj.Data.Attributes.Content);
            Assert.That(newObj.Data.Attributes.ButtonName == obj.Data.Attributes.ButtonName);
            Assert.That(newObj.Data.Attributes.Buttons.Count == obj.Data.Attributes.Buttons.Count);
            Assert.That(newObj.Data.Relationships.Organisation.Data.Id == obj.Data.Relationships.Organisation.Data.Id);
            Assert.That(newObj.Data.Relationships.MicroGame.Data.Id == obj.Data.Relationships.MicroGame.Data.Id);
        }
    }
}