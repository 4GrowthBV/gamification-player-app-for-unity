using System;
using System.Collections.Generic;
using System.Linq;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.DTO.LoginToken;
using GamificationPlayer.DTO.ModuleSession;
using GamificationPlayer.DTO.Chat;
using GamificationPlayer.Session;
using NUnit.Framework;
using UnityEngine;

namespace GamificationPlayer.Tests
{
    public class NonPersistentLogDataTest
    {
        [Test]
        public void TestRemoveListener()
        {
            var dto = new ModuleSessionStartedDTO();

            dto.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSessionStarted";

            var nonPersistentSessionData = new NonPersistentLogData();

            string testChallengeSessionId = default;
            string testModuleId = default;

            void challengeSessionIdMethod(object id)
            {
                testChallengeSessionId = (string)id;
            }

            nonPersistentSessionData.ListenTo<ChallengeSessionId>(challengeSessionIdMethod);

            nonPersistentSessionData.ListenTo<ModuleId>((id) =>
            {
                testModuleId = (string)id;
            });

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.AreEqual(testChallengeSessionId, dto.data.attributes.challenge_session_id);
            Assert.AreEqual(testModuleId, dto.data.attributes.module_id);

            nonPersistentSessionData.RemoveListener(challengeSessionIdMethod);

            dto = new ModuleSessionStartedDTO();

            dto.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSessionStarted";

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.AreNotEqual(testChallengeSessionId, dto.data.attributes.challenge_session_id);
            Assert.AreEqual(testModuleId, dto.data.attributes.module_id);
        }

        [Test]
        public void TestListen()
        {
            var dto = new ModuleSessionStartedDTO();

            dto.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSessionStarted";

            var nonPersistentSessionData = new NonPersistentLogData();

            string testCampaignId = default;
            string testChallengeSessionId = default;
            string testModuleId = default;
            string testBattleAvailableFrom = default;

            nonPersistentSessionData.ListenTo<ChallengeSessionId>((id) =>
            {
                testChallengeSessionId = (string)id;
            });

            nonPersistentSessionData.ListenTo<ModuleId>((id) =>
            {
                testModuleId = (string)id;
            });

            nonPersistentSessionData.ListenTo<CampaignId>((id) =>
            {
                testCampaignId = (string)id;
            });

            nonPersistentSessionData.ListenTo<BattleAvailableFrom>((id) =>
            {
                testCampaignId = (string)id;
                testChallengeSessionId = (string)id;
                testModuleId = (string)id;
                testBattleAvailableFrom = (string)id;
            });

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.AreEqual(testCampaignId, dto.data.attributes.campaign_id);
            Assert.AreEqual(testChallengeSessionId, dto.data.attributes.challenge_session_id);
            Assert.AreEqual(testModuleId, dto.data.attributes.module_id);
            Assert.AreEqual(testBattleAvailableFrom, default);
        }

        [Test]
        public void TestAdd()
        {
            var dto = new StandardDTO();

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.LogData.Count() == 1);
        }

        [Test]
        public void TestAddMultiple()
        {
            var dto1 = new StandardDTO();
            var dto2 = new StandardDTO();

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(new ILoggableData[] { dto1.data, dto2.data });

            Assert.That(nonPersistentSessionData.LogData.Count() == 2);
        }

        [Test]
        public void TestTryGetLatestQueryableIdValue()
        {
            var dto = new ModuleSessionStartedDTO();

            dto.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_id = Guid.NewGuid().ToString();
            dto.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSessionStarted";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out var id))
            {
                Assert.AreEqual(dto.data.attributes.challenge_session_id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out id))
            {
                Assert.AreEqual(dto.data.attributes.module_id, id);
            }


            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out id))
            {
                Assert.AreEqual(dto.data.attributes.module_session_id, id);
            }
        }

        [Test]
        public void TestTryGetLatestQueryableDateTimeValue()
        {
            var dto = new UpdateModuleSessionResponseDTO();

            dto.data.id = Guid.NewGuid().ToString();
            dto.data.attributes.started_at = new DateTime(2000, 1, 1).ToString();
            dto.data.attributes.ended_at = new DateTime(2001, 1, 1).ToString();
            dto.data.attributes.completed_at = "null";
            dto.data.relationships.challenge_session.data.id = Guid.NewGuid().ToString();
            dto.data.relationships.module.data.id = Guid.NewGuid().ToString();

            dto.data.type = "moduleSession";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out var moduleId))
            {
                Assert.AreEqual(dto.data.relationships.module.data.id, moduleId);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out var challengeSessionId))
            {
                Assert.AreEqual(dto.data.relationships.challenge_session.data.id, challengeSessionId);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out var dateTime))
            {
                Assert.AreEqual(dto.data.attributes.started_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out dateTime))
            {
                Assert.AreEqual(dto.data.attributes.ended_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out dateTime))
            {
                Assert.AreEqual(dto.data.attributes.completed_at, dateTime);
            }
        }

        [Test]
        public void TestTryGetLatestQueryableTokenValue()
        {
            var dto = new GetLoginTokenResponseDTO();

            dto.data.attributes.token = "123456789";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(dto.data);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out var id))
            {
                Assert.AreEqual(dto.data.attributes.token, id);
            }
        }   

        [Test]
        public void TestTryGetLatestQueryableValueWithMultiple()
        {
            var dto00 = new MicroGameOpenedDTO();

            dto00.data.type = "fitnessContentOpened";
            dto00.data.attributes.identifier = Guid.NewGuid().ToString();

            var dto0 = new MicroGameOpenedDTO();

            dto0.data.type = "microGameOpened";
            dto0.data.attributes.identifier = Guid.NewGuid().ToString();


            var dto1 = new ModuleSessionStartedDTO();

            dto1.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto1.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto1.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto1.data.attributes.module_id = Guid.NewGuid().ToString();
            dto1.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto1.data.type = "moduleSessionStarted";

            var dto2 = new GetLoginTokenResponseDTO();

            dto2.data.attributes.token = "123456789";
            dto2.data.type = "login_token";

            var dto3 = new ModuleSessionStartedDTO();

            dto3.data.attributes.campaign_id = Guid.NewGuid().ToString();
            dto3.data.attributes.challenge_id = Guid.NewGuid().ToString();
            dto3.data.attributes.challenge_session_id = Guid.NewGuid().ToString();
            dto3.data.attributes.module_id = Guid.NewGuid().ToString();
            dto3.data.attributes.module_session_id = Guid.NewGuid().ToString();

            dto3.data.type = "moduleSessionStarted";

            var dto4 = new UpdateModuleSessionResponseDTO();

            dto4.data.id = Guid.NewGuid().ToString();
            dto4.data.attributes.started_at = new DateTime(2000, 1, 1).ToString();
            dto4.data.attributes.ended_at = new DateTime(2001, 1, 1).ToString();
            dto4.data.attributes.completed_at = "null";
            dto4.data.relationships.challenge_session.data.id = Guid.NewGuid().ToString();
            dto4.data.relationships.module.data.id = Guid.NewGuid().ToString();

            dto4.data.type = "moduleSession";

            var lastDTO = new PageViewDTO();

            lastDTO.data.attributes.organisation_id = Guid.NewGuid().ToString();
            lastDTO.data.attributes.user_id = Guid.NewGuid().ToString();

            lastDTO.data.type = "pageView";

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(new ILoggableData[] { dto00.data, dto0.data, dto1.data, dto2.data, dto3.data, dto4.data, lastDTO.data });    

            Assert.That(nonPersistentSessionData.LogData.Count() == 7);

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, MicroGameIdentifier>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, MicroGameIdentifier>(out var id))
            {
                Assert.AreEqual(dto0.data.attributes.identifier, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, UserId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, UserId>(out id))
            {
                Assert.AreEqual(lastDTO.data.attributes.user_id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, OrganisationId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, OrganisationId>(out id))
            {
                Assert.AreEqual(lastDTO.data.attributes.organisation_id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChallengeSessionId>(out id))
            {
                Assert.AreEqual(dto4.data.relationships.challenge_session.data.id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleId>(out id))
            {
                Assert.AreEqual(dto4.data.relationships.module.data.id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionId>(out id))
            {
                Assert.AreEqual(dto4.data.id, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, LoginToken>(out id))
            {
                Assert.AreEqual(dto2.data.attributes.token, id);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionStarted>(out var dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.started_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionEnded>(out dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.ended_at, dateTime);
            }

            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out dateTime));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ModuleSessionCompleted>(out dateTime))
            {
                Assert.AreEqual(dto4.data.attributes.completed_at, dateTime);
            }
        }

        [Test]
        public void TestListenToChatQueryableAttributes()
        {
            var messageDto = new CreateChatConversationMessageResponseDTO();
            messageDto.data = new CreateChatConversationMessageResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatConversationMessage",
                attributes = new CreateChatConversationMessageResponseDTO.MessageAttributes
                {
                    role = "user",
                    message = "Hello from listener test",
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var instructionDto = new CreateChatInstructionResponseDTO();
            instructionDto.data = new CreateChatInstructionResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatInstruction",
                attributes = new CreateChatInstructionResponseDTO.InstructionAttributes
                {
                    identifier = "listener_test",
                    instruction = "Test instruction for listener",
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var nonPersistentSessionData = new NonPersistentLogData();

            string testChatMessage = default;
            string testChatRole = default;
            string testChatInstruction = default;
            string testChatInstructionIdentifier = default;

            nonPersistentSessionData.ListenTo<ChatMessage>((message) =>
            {
                testChatMessage = (string)message;
            });

            nonPersistentSessionData.ListenTo<ChatRole>((role) =>
            {
                testChatRole = (string)role;
            });

            nonPersistentSessionData.ListenTo<ChatInstruction>((instruction) =>
            {
                testChatInstruction = (string)instruction;
            });

            nonPersistentSessionData.ListenTo<ChatInstructionIdentifier>((identifier) =>
            {
                testChatInstructionIdentifier = (string)identifier;
            });

            // Add message first
            nonPersistentSessionData.AddToLog(messageDto.data);

            Assert.AreEqual(testChatMessage, messageDto.data.attributes.message);
            Assert.AreEqual(testChatRole, messageDto.data.attributes.role);
            Assert.AreEqual(testChatInstruction, default); // Should still be default
            Assert.AreEqual(testChatInstructionIdentifier, default); // Should still be default

            // Add instruction
            nonPersistentSessionData.AddToLog(instructionDto.data);

            Assert.AreEqual(testChatInstruction, instructionDto.data.attributes.instruction);
            Assert.AreEqual(testChatInstructionIdentifier, instructionDto.data.attributes.identifier);
        }

        [Test]
        public void TestTryGetLatestChatQueryableValues()
        {
            var conversationDto = new CreateChatConversationResponseDTO();
            conversationDto.data = new CreateChatConversationResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatConversation",
                attributes = new CreateChatConversationResponseDTO.Attributes
                {
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var messageDto = new CreateChatConversationMessageResponseDTO();
            messageDto.data = new CreateChatConversationMessageResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatConversationMessage",
                attributes = new CreateChatConversationMessageResponseDTO.MessageAttributes
                {
                    role = "assistant",
                    message = "Test message for NonPersistent query",
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var predefinedMessageDto = new CreateChatPredefinedMessageResponseDTO();
            predefinedMessageDto.data = new CreateChatPredefinedMessageResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatPredefinedMessage",
                attributes = new CreateChatPredefinedMessageResponseDTO.PredefinedMessageAttributes
                {
                    identifier = "test_predefined",
                    content = "Predefined message content",
                    buttons = new string[] { "Yes", "No", "Maybe" },
                    button_name = "Yes",
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var profileDto = new CreateChatProfileResponseDTO();
            profileDto.data = new CreateChatProfileResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatProfile",
                attributes = new CreateChatProfileResponseDTO.ProfileAttributes
                {
                    profile = "test_profile",
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var nonPersistentSessionData = new NonPersistentLogData();

            nonPersistentSessionData.AddToLog(conversationDto.data);
            nonPersistentSessionData.AddToLog(messageDto.data);
            nonPersistentSessionData.AddToLog(predefinedMessageDto.data);
            nonPersistentSessionData.AddToLog(profileDto.data);

            Assert.That(nonPersistentSessionData.LogData.Count() == 4);

            // Test ChatConversationId
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatConversationId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatConversationId>(out var conversationId))
            {
                Assert.AreEqual(conversationDto.data.id, conversationId);
            }

            // Test ChatMessage
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatMessage>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatMessage>(out var message))
            {
                Assert.AreEqual(messageDto.data.attributes.message, message);
            }

            // Test ChatRole
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatRole>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatRole>(out var role))
            {
                Assert.AreEqual(messageDto.data.attributes.role, role);
            }

            // Test ChatConversationMessageId
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatConversationMessageId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatConversationMessageId>(out var messageId))
            {
                Assert.AreEqual(messageDto.data.id, messageId);
            }

            // Test ChatPredefinedMessageId
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageId>(out var predefinedMessageId))
            {
                Assert.AreEqual(predefinedMessageDto.data.id, predefinedMessageId);
            }

            // Test ChatPredefinedMessageIdentifier
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageIdentifier>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageIdentifier>(out var predefinedIdentifier))
            {
                Assert.AreEqual(predefinedMessageDto.data.attributes.identifier, predefinedIdentifier);
            }

            // Test ChatPredefinedMessageContent
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageContent>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageContent>(out var content))
            {
                Assert.AreEqual(predefinedMessageDto.data.attributes.content, content);
            }

            // Test ChatPredefinedMessageButtonName
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageButtonName>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatPredefinedMessageButtonName>(out var buttonName))
            {
                Assert.AreEqual(predefinedMessageDto.data.attributes.button_name, buttonName);
            }

            // Test ChatProfile
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatProfile>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatProfile>(out var profile))
            {
                Assert.AreEqual(profileDto.data.attributes.profile, profile);
            }

            // Test ChatProfileId
            Assert.That(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatProfileId>(out _));
            if(nonPersistentSessionData.TryGetLatestQueryableValue<string, ChatProfileId>(out var profileId))
            {
                Assert.AreEqual(profileDto.data.id, profileId);
            }
        }

        [Test]
        public void TestRemoveChatListeners()
        {
            var messageDto1 = new CreateChatConversationMessageResponseDTO();
            messageDto1.data = new CreateChatConversationMessageResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatConversationMessage",
                attributes = new CreateChatConversationMessageResponseDTO.MessageAttributes
                {
                    role = "user",
                    message = "First message",
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var messageDto2 = new CreateChatConversationMessageResponseDTO();
            messageDto2.data = new CreateChatConversationMessageResponseDTO.Data
            {
                id = Guid.NewGuid().ToString(),
                type = "chatConversationMessage",
                attributes = new CreateChatConversationMessageResponseDTO.MessageAttributes
                {
                    role = "assistant",
                    message = "Second message",
                    created_at = DateTime.Now.ToString(),
                    updated_at = DateTime.Now.ToString()
                }
            };

            var nonPersistentSessionData = new NonPersistentLogData();

            string testChatMessage = default;
            string testChatRole = default;

            void chatMessageMethod(object message)
            {
                testChatMessage = (string)message;
            }

            nonPersistentSessionData.ListenTo<ChatMessage>(chatMessageMethod);

            nonPersistentSessionData.ListenTo<ChatRole>((role) =>
            {
                testChatRole = (string)role;
            });

            // Add first message
            nonPersistentSessionData.AddToLog(messageDto1.data);

            Assert.AreEqual(testChatMessage, messageDto1.data.attributes.message);
            Assert.AreEqual(testChatRole, messageDto1.data.attributes.role);

            // Remove chat message listener
            nonPersistentSessionData.RemoveListener(chatMessageMethod);

            // Add second message
            nonPersistentSessionData.AddToLog(messageDto2.data);

            // Chat message should not have changed (listener removed)
            Assert.AreNotEqual(testChatMessage, messageDto2.data.attributes.message);
            Assert.AreEqual(testChatMessage, messageDto1.data.attributes.message); // Still the first message

            // Chat role should have changed (listener still active)
            Assert.AreEqual(testChatRole, messageDto2.data.attributes.role);
        }
    }
}
