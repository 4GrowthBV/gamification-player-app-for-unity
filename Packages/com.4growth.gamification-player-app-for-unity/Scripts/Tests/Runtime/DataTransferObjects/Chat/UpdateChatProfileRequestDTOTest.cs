using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class UpdateChatProfileRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var profile = "Updated chat profile content";

            var obj = UpdateChatProfileRequestDTO.Create(profile);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_profile", obj.Data.Type);
            Assert.NotNull(obj.Data.Attributes);

            Assert.AreEqual(profile, obj.Data.Attributes.Profile);
        }

        [Test]
        public void TestToJSON()
        {
            var profile = "This is an updated profile";

            var obj = UpdateChatProfileRequestDTO.Create(profile);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(profile));
        }

        [Test]
        public void TestFromJSON()
        {
            var profile = "Updated test profile content";

            var obj = UpdateChatProfileRequestDTO.Create(profile);

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateChatProfileRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Attributes.Profile == obj.Data.Attributes.Profile);
        }
    }
}