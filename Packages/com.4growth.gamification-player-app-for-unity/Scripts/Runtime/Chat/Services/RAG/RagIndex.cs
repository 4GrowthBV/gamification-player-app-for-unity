using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationPlayer
{
    public class RagIndex
    {
        public readonly List<RagChunk> Chunks;
        private readonly int D;

        public RagIndex(List<RagChunk> chunks, int embeddingDim)
        {
            Chunks = chunks;
            D = embeddingDim;
            // Ensure normalized embeddings for fast cosine = dot
            foreach (var c in Chunks) NormalizeInPlace(c.Embedding);
        }

        public List<RagHit> Search(float[] queryEmbedding, int topK = 5)
        {
            NormalizeInPlace(queryEmbedding);
            var hits = new List<RagHit>(topK);

            float worst = -2f;
            int worstIdx = -1;

            for (int i = 0; i < Chunks.Count; i++)
            {
                var s = Dot(queryEmbedding, Chunks[i].Embedding);
                if (hits.Count < topK)
                {
                    hits.Add(new RagHit { Chunk = Chunks[i], Score = s });
                    if (s < worst || hits.Count == 1)
                    {
                        worst = s; worstIdx = hits.Count - 1;
                    }
                }
                else if (s > worst)
                {
                    hits[worstIdx] = new RagHit { Chunk = Chunks[i], Score = s };
                    // recompute worst
                    worst = hits[0].Score; worstIdx = 0;
                    for (int h = 1; h < hits.Count; h++)
                        if (hits[h].Score < worst) { worst = hits[h].Score; worstIdx = h; }
                }
            }

            hits.Sort((a, b) => b.Score.CompareTo(a.Score));
            return hits;
        }

        /* Persistence (fast binary) */

        public void Save(string path)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs, Encoding.UTF8);

            bw.Write(Chunks.Count);
            bw.Write(D);

            foreach (var c in Chunks)
            {
                bw.Write(c.DocId ?? "");
                bw.Write(c.Order);
                bw.Write(c.Text ?? "");
                // embedding
                bw.Write(D);
                for (int i = 0; i < D; i++) bw.Write(c.Embedding[i]);
            }
        }

        public static RagIndex Load(TextAsset textAsset)
        {
            using var fs = new MemoryStream(textAsset.bytes);
            using var br = new BinaryReader(fs, Encoding.UTF8);

            int count = br.ReadInt32();
            int D = br.ReadInt32();
            var chunks = new List<RagChunk>(count);

            for (int i = 0; i < count; i++)
            {
                var docId = br.ReadString();
                var order = br.ReadInt32();
                var text = br.ReadString();
                int d = br.ReadInt32();
                var emb = new float[d];
                for (int j = 0; j < d; j++) emb[j] = br.ReadSingle();

                chunks.Add(new RagChunk { DocId = docId, Order = order, Text = text, Embedding = emb });
            }

            return new RagIndex(chunks, D);
        }

        /* Math */

        private static void NormalizeInPlace(float[] v)
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++) sum += v[i] * v[i];
            float inv = (float)(1.0 / (Math.Sqrt(sum) + 1e-9));
            for (int i = 0; i < v.Length; i++) v[i] *= inv;
        }

        private static float Dot(float[] a, float[] b)
        {
            double s = 0;
            for (int i = 0; i < a.Length; i++) s += a[i] * b[i];
            return (float)s;
        }
    }
}