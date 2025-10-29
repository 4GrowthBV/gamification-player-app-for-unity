using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class UpdateChatPredefinedMessageRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var content = "Updated message content";
            var buttons = new List<string> { "New Option 1", "New Option 2" };
            var buttonName = "Updated Actions";

            var obj = UpdateChatPredefinedMessageRequestDTO.Create(content, buttons, buttonName);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_predefined_message", obj.Data.Type);
            Assert.NotNull(obj.Data.Attributes);

            Assert.AreEqual(content, obj.Data.Attributes.Content);
            Assert.AreEqual(buttons, obj.Data.Attributes.Buttons);
            Assert.AreEqual(buttonName, obj.Data.Attributes.ButtonName);
        }

        [Test]
        public void TestToJSON()
        {
            var content = "Test updated content";
            var buttons = new List<string> { "Action1", "Action2", "Action3" };
            var buttonName = "Test Actions";

            var obj = UpdateChatPredefinedMessageRequestDTO.Create(content, buttons, buttonName);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(content));
            Assert.That(json.Contains(buttonName));
            Assert.That(json.Contains("Action1"));
            Assert.That(json.Contains("Action2"));
            Assert.That(json.Contains("Action3"));
        }

        [Test]
        public void TestFromJSON()
        {
            var content = "Updated test content";
            var buttons = new List<string> { "Button A", "Button B" };
            var buttonName = "Test Button Name";

            var obj = UpdateChatPredefinedMessageRequestDTO.Create(content, buttons, buttonName);

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateChatPredefinedMessageRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Attributes.Content == obj.Data.Attributes.Content);
            Assert.That(newObj.Data.Attributes.ButtonName == obj.Data.Attributes.ButtonName);
            Assert.That(newObj.Data.Attributes.Buttons.Count == obj.Data.Attributes.Buttons.Count);
            Assert.That(newObj.Data.Attributes.Buttons[0] == obj.Data.Attributes.Buttons[0]);
            Assert.That(newObj.Data.Attributes.Buttons[1] == obj.Data.Attributes.Buttons[1]);
        }
    }
}