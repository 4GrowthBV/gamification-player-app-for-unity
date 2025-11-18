using System;

namespace GamificationPlayer.Chat.DTO
{
    [Serializable]
    public class WebButton
    {
        public string identifier;
        public string text;

        public WebButton(ChatManager.Button button)
        {
            identifier = button.identifier;
            text = button.text;
        }
    }
}