using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class UpdateChatInstructionRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var instruction = "Updated instruction content";

            var obj = UpdateChatInstructionRequestDTO.Create(instruction);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_instruction", obj.Data.Type);
            Assert.NotNull(obj.Data.Attributes);

            Assert.AreEqual(instruction, obj.Data.Attributes.Instruction);
        }

        [Test]
        public void TestToJSON()
        {
            var instruction = "This is an updated instruction";

            var obj = UpdateChatInstructionRequestDTO.Create(instruction);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(instruction));
        }

        [Test]
        public void TestFromJSON()
        {
            var instruction = "Updated test instruction";

            var obj = UpdateChatInstructionRequestDTO.Create(instruction);

            var json = obj.ToJson();
            var newObj = json.FromJson<UpdateChatInstructionRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Attributes.Instruction == obj.Data.Attributes.Instruction);
        }
    }
}