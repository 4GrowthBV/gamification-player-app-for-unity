using System;
using System.Collections.Generic;
using GamificationPlayer.DTO.ExternalEvents;
using GamificationPlayer.Session;

namespace GamificationPlayer.Tests
{
    public class SessionLogDataMock : ISessionLogData
    {
        public void AddToLog(ILoggableData dto, bool clearMissingPersistentData = true)
        {
        }

        public void AddToLog(IEnumerable<ILoggableData> dto, bool clearMissingPersistentData = true)
        {
        }

        public bool TryGetLatestChallengeSessionId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestDeviceFlowId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestSubdomain(out string subdomain)
        {
            subdomain = Guid.NewGuid().ToString();

            return true;
        }

        public bool TryGetLatestFitnessContentIdentifier(out string identifier)
        {
            identifier = Guid.NewGuid().ToString();

            return true;
        }

        public bool TryGetLatestId<TAttribute>(out Guid id) where TAttribute : IQueryable
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestLoginToken(out string token)
        {
            token = "123";

            return true;
        }

        public bool TryGetLatestMicroGameIdentifier(out string identifier)
        {
            identifier = Guid.NewGuid().ToString();

            return true;
        }

        public bool TryGetLatestModuleId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestModuleSessionCompleted(out DateTime dateTime)
        {
            dateTime = default;

            return false;
        }

        public bool TryGetLatestModuleSessionEnded(out DateTime dateTime)
        {
            dateTime = default;

            return false;
        }

        public bool TryGetLatestModuleSessionId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        public bool TryGetLatestModuleSessionStarted(out DateTime dateTime)
        {
            dateTime = new DateTime(2000, 1, 1);

            return true;
        }

        public bool TryGetLatestOrganisationId(out Guid id)
        {
            id = new Guid("7bcfc94d-8a06-4fa8-b5ff-b35415a65b16");

            return true;
        }

        public bool TryGetLatestServerTime(out DateTime dateTime)
        {
            dateTime = DateTime.Now;

            return true;
        } 

        public bool TryGetLatestUserId(out Guid id)
        {
            id = new Guid("46f1d6fc-36b0-48fe-8ffd-e8dfc1a15eba");

            return true;
        }

        public bool TryGetWhenServerTime(out float realtimeSinceStartup)
        {
            realtimeSinceStartup = 0f;

            return true;
        }

        public bool TryGetLatest<TQueryable>(out string value) 
            where TQueryable : IQueryable
        {
            value = string.Empty;

            return true;
        }

        public bool TryGetLatestBattleSessionId(out Guid id)
        {
            id = new Guid("46f1d6fc-36b0-48fe-8ffd-e8dfc1a15eba");

            return true;
        }

        public bool TryGetLatestMicroGamePayload(out MicroGamePayload microGamePayload)
        {
            microGamePayload = new MicroGamePayload();

            return true;
        }

        public void ListenTo<T>(Action<object> callback) where T : IQueryable
        {
            
        }

        public bool TryGetLatestMicroGameId(out Guid id)
        {
            id = Guid.NewGuid();

            return true;
        }

        // Chat-specific mock implementations
        public bool TryGetLatestChatConversationId(out Guid id)
        {
            id = Guid.NewGuid();
            return true;
        }

        public bool TryGetLatestChatConversationMessageId(out Guid id)
        {
            id = Guid.NewGuid();
            return true;
        }

        public bool TryGetLatestChatInstructionId(out Guid id)
        {
            id = Guid.NewGuid();
            return true;
        }

        public bool TryGetLatestChatProfileId(out Guid id)
        {
            id = Guid.NewGuid();
            return true;
        }

        public bool TryGetLatestChatRole(out string role)
        {
            role = "assistant";
            return true;
        }

        public bool TryGetLatestChatMessage(out string message)
        {
            message = "Hello! How can I help you today?";
            return true;
        }

        public bool TryGetLatestChatPredefinedMessageId(out Guid id)
        {
            id = Guid.NewGuid();
            return true;
        }

        public bool TryGetLatestChatInstructionIdentifier(out string identifier)
        {
            identifier = "welcome_instruction";
            return true;
        }

        public bool TryGetLatestChatInstruction(out string instruction)
        {
            instruction = "You are a helpful AI assistant for a gamification platform.";
            return true;
        }

        public bool TryGetLatestChatPredefinedMessageIdentifier(out string identifier)
        {
            identifier = "welcome_message";
            return true;
        }

        public bool TryGetLatestChatPredefinedMessageContent(out string content)
        {
            content = "Welcome! How would you like to start?";
            return true;
        }

        public bool TryGetLatestChatPredefinedMessageButtons(out string[] buttons)
        {
            buttons = new string[] { "Get Started", "Learn More", "Help" };
            return true;
        }

        public bool TryGetLatestChatPredefinedMessageButtonName(out string buttonName)
        {
            buttonName = "Get Started";
            return true;
        }

        public bool TryGetLatestChatProfile(out string profile)
        {
            profile = "Friendly AI Assistant with gaming knowledge";
            return true;
        }
    }
}
