using System;
using UnityEngine;
using GamificationPlayer.Chat.Services;
/*
 * LOCAL RAG FOR UNITY (WebGL/Standalone)
 * --------------------------------------
 * - Uses MiniLM sentence embeddings (all-MiniLM-L6-v2, 384 dims)
 * - WordPiece tokenizer (your working version)
 * - Document chunking with overlap
 * - Cosine similarity retrieval
 * - Binary index persistence (fast load)
 *
 * HOW TO USE
 * 1) Assign modelAsset (MiniLM ONNX), vocabAsset (vocab.txt), and sourceDocs[] (TextAssets).
 * 2) Play. It builds an index (or loads from cache), then runs a demo query.
 * 3) Call Rag.Search("jouw vraag", topK) for retrieval-only,
 *    or Rag.Ask("jouw vraag", topK) to get a stitched context block.
 */

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "RagSettings", menuName = "AIChat/RAG", order = 1)]
    public class RagSettings : ScriptableObject
    {
        public RAGType rAGType;
        public string agentName;

        [Header("Documents to Index")]
        public TextAsset[] sourceDocs;

        [Header("Chunking")]
        public int ChunkMaxTokens = 160;     // ~ short paragraph
        public int ChunkOverlapTokens = 24;  // token overlap

        [Header("Index")]
        public int EmbeddingDim = 384;
    }
}