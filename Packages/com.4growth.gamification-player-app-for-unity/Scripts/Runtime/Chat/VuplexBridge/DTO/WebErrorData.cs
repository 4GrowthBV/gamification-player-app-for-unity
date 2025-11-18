using System;

namespace GamificationPlayer.Chat.DTO
{
    [Serializable]
    public class WebErrorData
    {
        public string error;
        public string timestamp;

        public WebErrorData(string error)
        {
            this.error = error;
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}