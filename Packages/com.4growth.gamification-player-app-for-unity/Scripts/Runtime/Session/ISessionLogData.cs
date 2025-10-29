using System;
using System.Collections.Generic;
using GamificationPlayer.DTO.ExternalEvents;

namespace GamificationPlayer
{
    public interface ISessionLogData
    {
        public bool TryGetLatestId<TQueryable>(out Guid id)
            where TQueryable : Session.IQueryable;
            
        public bool TryGetLatest<TQueryable>(out string value)
            where TQueryable : Session.IQueryable;

        public bool TryGetLatestSubdomain(out string subdomain);

        public bool TryGetLatestMicroGameIdentifier(out string identifier);

        public bool TryGetLatestLoginToken(out string token);

        public bool TryGetLatestModuleSessionId(out Guid id);

        public bool TryGetLatestBattleSessionId(out Guid id);

        public bool TryGetLatestMicroGamePayload(out MicroGamePayload microGamePayload);

        public bool TryGetLatestModuleSessionStarted(out DateTime dateTime);

        public bool TryGetLatestModuleSessionEnded(out DateTime dateTime);

        public bool TryGetLatestModuleSessionCompleted(out DateTime dateTime);

        public bool TryGetLatestServerTime(out DateTime dateTime);

        public bool TryGetWhenServerTime(out float realtimeSinceStartup);
        
        public bool TryGetLatestModuleId(out Guid id);

        public bool TryGetLatestChallengeSessionId(out Guid id);

        public bool TryGetLatestDeviceFlowId(out Guid id);

        public bool TryGetLatestOrganisationId(out Guid id);

        public bool TryGetLatestUserId(out Guid id);

        public bool TryGetLatestMicroGameId(out Guid id);

        // Chat-specific methods
        public bool TryGetLatestChatConversationId(out Guid id);

        public bool TryGetLatestChatConversationMessageId(out Guid id);

        public bool TryGetLatestChatInstructionId(out Guid id);

        public bool TryGetLatestChatProfileId(out Guid id);

        public bool TryGetLatestChatRole(out string role);

        public bool TryGetLatestChatMessage(out string message);

        public bool TryGetLatestChatPredefinedMessageId(out Guid id);

        public bool TryGetLatestChatInstructionIdentifier(out string identifier);

        public bool TryGetLatestChatInstruction(out string instruction);

        public bool TryGetLatestChatPredefinedMessageIdentifier(out string identifier);

        public bool TryGetLatestChatPredefinedMessageContent(out string content);

        public bool TryGetLatestChatPredefinedMessageButtons(out string[] buttons);

        public bool TryGetLatestChatPredefinedMessageButtonName(out string buttonName);

        public bool TryGetLatestChatProfile(out string profile);

        public void AddToLog(ILoggableData dto, bool clearMissingPersistentData = true);

        public void AddToLog(IEnumerable<ILoggableData> dto, bool clearMissingPersistentData = true);

        public void ListenTo<T>(Action<object> callback) where T : Session.IQueryable;
    }
}