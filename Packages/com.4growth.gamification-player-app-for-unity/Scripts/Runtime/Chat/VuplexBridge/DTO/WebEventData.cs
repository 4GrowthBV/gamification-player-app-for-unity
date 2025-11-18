using System;

namespace GamificationPlayer.Chat.DTO
{
    [Serializable]
    public class WebEventData
    {
        public string eventType;
        public object data;
        public string timestamp;

        public WebEventData(SentToWebEventType eventType, object data)
        {
            this.eventType = eventType.ToString();
            this.data = data;
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}