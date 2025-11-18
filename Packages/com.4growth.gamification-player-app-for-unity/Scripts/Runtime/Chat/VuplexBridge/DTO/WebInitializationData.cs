using System;
using System.Collections.Generic;

namespace GamificationPlayer.Chat.DTO
{
    [Serializable]
    public class WebInitializationData
    {
        public WebChatMessage[] conversationHistory;
        public string timestamp;
        public bool expectNewMessage;

        public WebInitializationData(List<ChatManager.ChatMessage> history, bool expectNewMessage = false)
        {
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            this.expectNewMessage = expectNewMessage;

            if (history != null)
            {
                conversationHistory = new WebChatMessage[history.Count];
                for (int i = 0; i < history.Count; i++)
                {
                    conversationHistory[i] = new WebChatMessage(history[i]);
                }
            }
        }
    }
}