using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity.InferenceEngine;
using GamificationPlayer.Chat.Services;

namespace GamificationPlayer
{
    public class RagInitializer : MonoBehaviour
    {
        [Header("MiniLM (all-MiniLM-L6-v2)")]
        [SerializeField] private ModelAsset modelAsset;
        [SerializeField] private TextAsset vocabAsset;
        [SerializeField] private BackendType backend = BackendType.CPU; // GPUCompute also fine

        [Header("RAG Settings")]
        [SerializeField] private RagSettings[] ragSettings;

        private List<Rag> allRags = new List<Rag>();

        void Awake()
        {
            foreach (var settings in ragSettings)
            {
                var rag = CreateRag(settings);
                allRags.Add(rag);
            }
        }

        private Rag CreateRag(RagSettings settings)
        {
            // 1) Init embedder (loads model + builds functional mean-pooling graph)
            var embedder = new MiniLMEmbedder(modelAsset, vocabAsset, backend, settings.EmbeddingDim);
            RagIndex index;

            // 2) Load or build index
            string indexPath = Path.Combine(Application.persistentDataPath, $"rag_index_{settings.agentName}_{settings.rAGType}.bin");
            if (File.Exists(indexPath))
            {
                index = RagIndex.Load(indexPath);
                Debug.Log($"[RAG] Loaded index: {index.Chunks.Count} chunks from {indexPath}");
            }
            else
            {
                // Build chunks
                var chunker = new RagChunker(embedder.TokenizeCount);
                var chunks = new List<RagChunk>();

                for (int d = 0; d < settings.sourceDocs.Length; d++)
                {
                    string docId = settings.sourceDocs[d].name;
                    string text = settings.sourceDocs[d].text;
                    foreach (var (chunkText, order) in chunker.ChunkByTokens(text, settings.ChunkMaxTokens, settings.ChunkOverlapTokens))
                    {
                        chunks.Add(new RagChunk
                        {
                            DocId = docId,
                            Order = order,
                            Text = chunkText
                        });
                    }
                }

                // Embed chunks (batch to reduce allocations)
                embedder.EmbedInPlace(chunks);

                index = new RagIndex(chunks, settings.EmbeddingDim);
                index.Save(indexPath);
                Debug.Log($"[RAG] Built & saved index: {index.Chunks.Count} chunks -> {indexPath}");
            }

            // 3) Create RAG runtime
            return new Rag(settings.rAGType, settings.agentName, embedder, index);
        }

        public List<IRAG> GetAllRags()
        {
            return new List<IRAG>(allRags);
        }

        void OnDestroy()
        {
            foreach (var rag in allRags)
            {
                rag?.Dispose();
            }
        }

        static string Trim(string s) => s.Length <= 160 ? s : s.Substring(0, 160) + "...";
    }
}