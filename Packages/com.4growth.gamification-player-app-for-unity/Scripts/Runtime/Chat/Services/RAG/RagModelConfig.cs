using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.InferenceEngine;
using GamificationPlayer.Chat.Services;
using System.Linq;

namespace GamificationPlayer
{
    /// <summary>
    /// Configuration for RAG model assets and paths.
    /// Defines where to find model files in StreamingAssets.
    /// </summary>
    [CreateAssetMenu(fileName = "RagModelConfig", menuName = "AIChat/RAG Model Config", order = 0)]
    public class RagModelConfig : ScriptableObject
    {
        [Header("Model Files (in StreamingAssets)")]
        [Tooltip("Path to MiniLM model in StreamingAssets (supports .onnx, .sentis formats, e.g., 'RAG/Models/all-MiniLM-L6-v2.sentis')")]
        public string modelPath = "RAG/Models/all-MiniLM-L6-v2.sentis";
        
        [Tooltip("Path to vocabulary file in StreamingAssets (e.g., 'RAG/Models/all-MiniLM-L6-v2_vocab.txt')")]
        public string vocabPath = "RAG/Models/all-MiniLM-L6-v2_vocab.txt";
        
        [Header("Model Settings")]
        public BackendType backend = BackendType.CPU;
        public int embeddingDim = 384;
        
        /// <summary>
        /// Get the full StreamingAssets path for the model file
        /// </summary>
        public string GetModelFullPath()
        {
            return Path.Combine(Application.streamingAssetsPath, modelPath);
        }
        
        /// <summary>
        /// Get the full StreamingAssets path for the vocab file
        /// </summary>
        public string GetVocabFullPath()
        {
            return Path.Combine(Application.streamingAssetsPath, vocabPath);
        }
        
        /// <summary>
        /// Validate that the configured files exist
        /// </summary>
        public bool ValidateFiles(out string error)
        {
            error = null;
            
            if (string.IsNullOrEmpty(modelPath))
            {
                error = "Model path is not configured";
                return false;
            }
            
            if (string.IsNullOrEmpty(vocabPath))
            {
                error = "Vocab path is not configured";
                return false;
            }
            
            string modelFullPath = GetModelFullPath();
            if (!File.Exists(modelFullPath))
            {
                error = $"Model file not found: {modelFullPath}";
                return false;
            }
            
            string vocabFullPath = GetVocabFullPath();
            if (!File.Exists(vocabFullPath))
            {
                error = $"Vocab file not found: {vocabFullPath}";
                return false;
            }
            
            if (embeddingDim <= 0)
            {
                error = "Embedding dimension must be positive";
                return false;
            }
            
            return true;
        }
    }
}