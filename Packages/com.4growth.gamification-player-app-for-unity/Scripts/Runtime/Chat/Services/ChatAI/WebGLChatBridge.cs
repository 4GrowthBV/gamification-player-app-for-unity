using GamificationPlayer.Chat.Services;
using UnityEngine;

namespace GamificationPlayer
{    
    public class WebGLChatBridge : MonoBehaviour
    {
        public static System.Action<string> OnStreamChunk;
        public static System.Action<AIResponseResult> OnStreamComplete;

        public void OnStreamChunkJS(string text)
        {           
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnStreamChunk?.Invoke(text);
            });
        }

        public void OnStreamCompleteJS(string text)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnStreamComplete?.Invoke(new AIResponseResult(text));
            });
        }
    }
}
