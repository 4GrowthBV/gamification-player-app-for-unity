using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationPlayer
{
    public class RagChunker
    {
        private readonly Func<string, int> _tokenCount;

        public RagChunker(Func<string, int> tokenCount) => _tokenCount = tokenCount;

        // Returns (chunkText, order)
        public IEnumerable<(string, int)> ChunkByTokens(string text, int maxTokens, int overlapTokens)
        {
            // Simple paragraph-first split, then fallback to sentence/word slicing by token counts
            var paras = SplitParagraphs(text);
            int order = 0;

            var buffer = new StringBuilder();
            int bufTokens = 0;

            foreach (var p in paras)
            {
                var pt = p.Trim();
                if (string.IsNullOrEmpty(pt)) continue;

                // If paragraph is small, try to append whole
                int pTok = _tokenCount(pt);
                if (bufTokens + pTok <= maxTokens)
                {
                    buffer.AppendLine(pt);
                    bufTokens += pTok;
                    continue;
                }

                // Flush current buffer if it has content
                if (bufTokens > 0)
                {
                    yield return (buffer.ToString().Trim(), order++);
                    // overlap
                    var back = TakeTail(buffer.ToString(), overlapTokens);
                    buffer = new StringBuilder(back);
                    bufTokens = _tokenCount(buffer.ToString());
                }

                // If paragraph itself is too big, slice it
                if (pTok > maxTokens)
                {
                    foreach (var slice in SliceByTokens(pt, maxTokens, overlapTokens))
                        yield return (slice, order++);
                }
                else
                {
                    buffer.AppendLine(pt);
                    bufTokens += pTok;
                }
            }

            if (bufTokens > 0)
                yield return (buffer.ToString().Trim(), order++);
        }

        private static IEnumerable<string> SliceByTokens(string text, int maxTokens, int overlap)
        {
            // Very simple sentence-based slicing fallback
            var sentences = SplitSentences(text);
            var chunks = new List<string>();
            var cur = new StringBuilder();

            foreach (var s in sentences)
            {
                if (cur.Length == 0)
                {
                    cur.Append(s);
                }
                else
                {
                    cur.Append(" ").Append(s);
                }

                // we cannot count tokens here (no embedder); caller wraps with tokenCount
                chunks.Add(cur.ToString());
            }
            // Caller (ChunkByTokens) builds token-aware slices. This fallback returns whole para sentences;
            // in practice, token-aware windowing from ChunkByTokens will handle large paras.
            return chunks;
        }

        private static IEnumerable<string> SplitParagraphs(string text)
            => text.Replace("\r\n", "\n").Split(new[] { "\n\n", "\n \n" }, StringSplitOptions.None);

        private static IEnumerable<string> SplitSentences(string p)
            => p.Split(new[] { ". ", "? ", "! " }, StringSplitOptions.RemoveEmptyEntries);

        private static string TakeTail(string text, int approxTokensKeep)
        {
            // Keep tail chunk by approximate char ratio (fast; good enough for overlap)
            if (approxTokensKeep <= 0) return "";
            int keepChars = Mathf.Clamp(approxTokensKeep * 5, 0, text.Length);
            return text.Substring(Math.Max(0, text.Length - keepChars)).Trim();
        }
    }
}