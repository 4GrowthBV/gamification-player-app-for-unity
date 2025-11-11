using System;
using System.Collections.Generic;

namespace GamificationPlayer
{
    /// <summary>
    /// Interface for embedding text into vector representations.
    /// Allows different implementations (file-based, asset-based, etc.).
    /// </summary>
    public interface IEmbedder : IDisposable
    {
        /// <summary>
        /// Embed a single text string into a vector representation.
        /// </summary>
        /// <param name="text">Text to embed</param>
        /// <returns>Embedding vector</returns>
        float[] Embed(string text);

        /// <summary>
        /// Embed chunks in-place by setting their Embedding property.
        /// </summary>
        /// <param name="chunks">List of chunks to embed</param>
        void EmbedInPlace(List<RagChunk> chunks);

        /// <summary>
        /// Count the number of tokens in text (for chunking).
        /// </summary>
        /// <param name="text">Text to tokenize</param>
        /// <returns>Number of tokens</returns>
        int TokenizeCount(string text);
    }
}