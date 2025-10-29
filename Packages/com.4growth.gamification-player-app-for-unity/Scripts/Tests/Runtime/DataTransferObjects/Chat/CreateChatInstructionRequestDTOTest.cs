using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GamificationPlayer.DTO.Chat;

namespace GamificationPlayer.Tests
{
    public class CreateChatInstructionRequestDTOTest
    {
        [Test]
        public void TestConstructor()
        {
            var identifier = "welcome_instruction";
            var instruction = "Welcome to our chat service";
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatInstructionRequestDTO.Create(identifier, instruction, organisationId, microGameId);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Data);
            Assert.AreEqual("chat_instruction", obj.Data.Type);
            Assert.NotNull(obj.Data.Attributes);
            Assert.NotNull(obj.Data.Relationships);

            Assert.AreEqual(identifier, obj.Data.Attributes.Identifier);
            Assert.AreEqual(instruction, obj.Data.Attributes.Instruction);

            Assert.AreEqual("organisation", obj.Data.Relationships.Organisation.Data.Type);
            Assert.AreEqual(organisationId.ToString(), obj.Data.Relationships.Organisation.Data.Id);

            Assert.AreEqual("micro_game", obj.Data.Relationships.MicroGame.Data.Type);
            Assert.AreEqual(microGameId.ToString(), obj.Data.Relationships.MicroGame.Data.Id);
        }

        [Test]
        public void TestToJSON()
        {
            var identifier = "test_instruction";
            var instruction = "This is a test instruction";
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatInstructionRequestDTO.Create(identifier, instruction, organisationId, microGameId);

            var json = obj.ToJson();

            Assert.That(json.Contains(obj.Data.Type));
            Assert.That(json.Contains(identifier));
            Assert.That(json.Contains(instruction));
            Assert.That(json.Contains(organisationId.ToString()));
            Assert.That(json.Contains(microGameId.ToString()));
            Assert.That(json.Contains("organisation"));
            Assert.That(json.Contains("micro_game"));
        }

        [Test]
        public void TestFromJSON()
        {
            var identifier = "test_instruction";
            var instruction = "Test instruction content";
            var organisationId = Guid.NewGuid();
            var microGameId = Guid.NewGuid();

            var obj = CreateChatInstructionRequestDTO.Create(identifier, instruction, organisationId, microGameId);

            var json = obj.ToJson();
            var newObj = json.FromJson<CreateChatInstructionRequestDTO>();

            Assert.That(newObj.Data.Type == obj.Data.Type);
            Assert.That(newObj.Data.Attributes.Identifier == obj.Data.Attributes.Identifier);
            Assert.That(newObj.Data.Attributes.Instruction == obj.Data.Attributes.Instruction);
            Assert.That(newObj.Data.Relationships.Organisation.Data.Id == obj.Data.Relationships.Organisation.Data.Id);
            Assert.That(newObj.Data.Relationships.MicroGame.Data.Id == obj.Data.Relationships.MicroGame.Data.Id);
        }
    }
}