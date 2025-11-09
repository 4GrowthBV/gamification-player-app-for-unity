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
                Debug.Log("WebGLChatBridge received stream chunk: " + text);

                OnStreamChunk?.Invoke(text);
            });
        }

        public void OnStreamCompleteJS(string text)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                Debug.Log("WebGLChatBridge stream complete: " + text);

                OnStreamComplete?.Invoke(new AIResponseResult(text));
            });
        }
    }
}
