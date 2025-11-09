using System;

namespace GamificationPlayer
{
    [Serializable]
    public class RagChunk
    {
        public string DocId;
        public int Order;
        public string Text;
        public float[] Embedding; // length = D
    }
}