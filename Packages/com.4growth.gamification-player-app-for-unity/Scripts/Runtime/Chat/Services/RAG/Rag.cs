using System.Text;
using System.Linq;
using System.Collections.Generic;
using GamificationPlayer.Chat.Services;

namespace GamificationPlayer
{
    public class Rag : IRAG
    {
        public RAGType RAGType { get { return rAGType; } }
        public string AgentName { get { return agentName; } }

        private readonly RAGType rAGType;
        private readonly string agentName;
        private readonly MiniLMEmbedder _embedder;
        private readonly RagIndex _index;

        public Rag(RAGType rAGType,
            string agentName,
            MiniLMEmbedder embedder,
            RagIndex index)
        {
            this.rAGType = rAGType;
            this.agentName = agentName;
            _embedder = embedder;
            _index = index;
        }

        public List<RagHit> Search(string query, int topK = 5)
        {
            var q = _embedder.Embed(query);
            return _index.Search(q, topK);
        }

        public string Ask(string query, int topK = 5, int maxChars = 1200)
        {
            var hits = Search(query, topK);
            // Stitch (sorted by doc & order to preserve flow)
            var grouped = hits
                .GroupBy(h => h.Chunk.DocId)
                .OrderByDescending(g => g.Max(h => h.Score))
                .SelectMany(g => g.OrderBy(h => h.Chunk.Order));

            var sb = new StringBuilder();
            sb.AppendLine("### Gevonden context (Top-K):");
            foreach (var h in grouped)
            {
                sb.AppendLine($"[bron: {h.Chunk.DocId} #{h.Chunk.Order} | score: {h.Score:F3}]");
                sb.AppendLine(h.Chunk.Text.Trim());
                sb.AppendLine();
                if (sb.Length > maxChars) break;
            }

            // You can now feed `sb.ToString()` as context to your UI/LLM.
            return sb.ToString();
        }

        public void Dispose()
        {
            _embedder?.Dispose();
        }
    }
}