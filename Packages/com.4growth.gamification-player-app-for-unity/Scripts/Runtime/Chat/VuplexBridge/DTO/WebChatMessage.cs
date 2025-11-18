using System;
using System.Collections.Generic;

namespace GamificationPlayer.Chat.DTO
{
    [Serializable]
    public class WebChatMessage
    {
        public string role;
        public string message;
        public WebButton[] buttons;
        public string timestamp;
        public string buttonName;
        public Dictionary<string, string> userActivityMetadata;

        public WebChatMessage(ChatManager.ChatMessage chatMessage)
        {
            role = chatMessage.role;
            message = chatMessage.message;
            timestamp = chatMessage.timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            buttonName = chatMessage.buttonName;
            userActivityMetadata = chatMessage.userActivityMetadata;

            if (chatMessage.buttons != null)
            {
                buttons = new WebButton[chatMessage.buttons.Length];
                for (int i = 0; i < chatMessage.buttons.Length; i++)
                {
                    buttons[i] = new WebButton(chatMessage.buttons[i]);
                }
            }
        }
    }
}