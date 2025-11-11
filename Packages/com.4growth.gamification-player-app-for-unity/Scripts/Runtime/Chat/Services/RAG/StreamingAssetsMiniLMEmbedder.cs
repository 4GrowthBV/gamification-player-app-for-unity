using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.InferenceEngine;
using UnityEngine.Networking;

namespace GamificationPlayer
{
    /// <summary>
    /// Enhanced MiniLM embedder that can load model and vocab from StreamingAssets files.
    /// No dependency on ModelAsset or TextAsset - works with pure file paths.
    /// </summary>
    public class StreamingAssetsMiniLMEmbedder : IEmbedder
    {
        private int FEATURES;
        private Worker worker;
        private string[] vocabTokens;

        // model expects 3 inputs: input_ids, attention_mask, token_type_ids
        // output: "last_hidden_state" [1, seq, FEATURES]
        private const int START_TOKEN = 101;
        private const int END_TOKEN = 102;

        /// <summary>
        /// Async factory method for WebGL-compatible creation
        /// </summary>
        public static async System.Threading.Tasks.Task<StreamingAssetsMiniLMEmbedder> CreateAsync(
            string modelFilePath, string vocabFilePath, BackendType backend, int features)
        {
            var embedder = new StreamingAssetsMiniLMEmbedder();
            await embedder.InitializeAsync(modelFilePath, vocabFilePath, backend, features);
            return embedder;
        }

        /// <summary>
        /// Private constructor for async creation
        /// </summary>
        private StreamingAssetsMiniLMEmbedder()
        {
        }

        /// <summary>
        /// Internal async initialization method
        /// </summary>
        private async System.Threading.Tasks.Task InitializeAsync(string modelFilePath, string vocabFilePath, BackendType backend, int features)
        {
            this.FEATURES = features;

#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL: Load files using UnityWebRequest
            await LoadFilesWebGL(modelFilePath, vocabFilePath, backend);
#else
            // Desktop: Load files synchronously
            LoadFilesDesktop(modelFilePath, vocabFilePath, backend);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private async System.Threading.Tasks.Task LoadFilesWebGL(string modelFilePath, string vocabFilePath, BackendType backend)
        {
            // Convert file paths to URLs for WebGL
            string modelUrl = modelFilePath.StartsWith("http") ? modelFilePath : 
                modelFilePath.Replace("\\", "/");
            string vocabUrl = vocabFilePath.StartsWith("http") ? vocabFilePath : 
                vocabFilePath.Replace("\\", "/");
            
            // Load vocabulary file
            var vocabRequest = UnityWebRequest.Get(vocabUrl);
            var vocabOperation = vocabRequest.SendWebRequest();
            
            while (!vocabOperation.isDone)
            {
                await System.Threading.Tasks.Task.Yield();
            }
            
            if (vocabRequest.result != UnityWebRequest.Result.Success)
            {
                vocabRequest.Dispose();
                throw new System.Exception($"Failed to load vocab from {vocabUrl}: {vocabRequest.error}");
            }
            
            string vocabText = vocabRequest.downloadHandler.text;
            this.vocabTokens = vocabText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            vocabRequest.Dispose();
            
            // Load model file
            var modelRequest = UnityWebRequest.Get(modelUrl);
            var modelOperation = modelRequest.SendWebRequest();
            
            while (!modelOperation.isDone)
            {
                await System.Threading.Tasks.Task.Yield();
            }
            
            if (modelRequest.result != UnityWebRequest.Result.Success)
            {
                modelRequest.Dispose();
                throw new System.Exception($"Failed to load model from {modelUrl}: {modelRequest.error}");
            }
            
            byte[] modelData = modelRequest.downloadHandler.data;
            
            // Load model from stream - clean and simple!
            using var memoryStream = new System.IO.MemoryStream(modelData);
            var baseModel = ModelLoader.Load(memoryStream);
            modelRequest.Dispose();
            
            CreateWorker(baseModel, backend);
        }
#endif

        private void LoadFilesDesktop(string modelFilePath, string vocabFilePath, BackendType backend)
        {
            // Load vocab from file
            if (!File.Exists(vocabFilePath))
                throw new FileNotFoundException($"Vocab file not found: {vocabFilePath}");
                
            string vocabText = File.ReadAllText(vocabFilePath);
            this.vocabTokens = vocabText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Load model from file
            if (!File.Exists(modelFilePath))
                throw new FileNotFoundException($"Model file not found: {modelFilePath}");

            // Direct file loading as per Unity documentation (supports .onnx, .sentis)
            var baseModel = ModelLoader.Load(modelFilePath);
            
            CreateWorker(baseModel, backend);
        }

        private void CreateWorker(Model baseModel, BackendType backend)
        {
            // Compose model graph that adds masked mean pool + L2 normalize to output
            var g = new FunctionalGraph();
            var inputs = g.AddInputs(baseModel);                         // input_ids, attention_mask, token_type_ids
            var lastHidden = Functional.Forward(baseModel, inputs)[0];   // "last_hidden_state"
            var attentionMask = inputs[1];

            var pooled = MeanPool(lastHidden, attentionMask, FEATURES);
            var normalized = L2Normalize(pooled);
            var compiled = g.Compile(normalized);

            this.worker = new Worker(compiled, backend);
        }



        /// <summary>
        /// Create embedder from RagModelConfig (async for WebGL compatibility)
        /// </summary>
        public static async System.Threading.Tasks.Task<StreamingAssetsMiniLMEmbedder> FromConfigAsync(RagModelConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

#if !UNITY_WEBGL || UNITY_EDITOR
            // Desktop: can validate files exist
            if (!config.ValidateFiles(out string error))
                throw new InvalidOperationException($"Invalid RAG model config: {error}");
#endif

            return await CreateAsync(
                config.GetModelFullPath(),
                config.GetVocabFullPath(),
                config.backend,
                config.embeddingDim
            );
        }

        /// <summary>
        /// Synchronous wrapper for editor
        /// </summary>
        public static StreamingAssetsMiniLMEmbedder FromConfig(RagModelConfig config)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            throw new InvalidOperationException("Use FromConfigAsync() for WebGL builds");
#else
            return FromConfigAsync(config).GetAwaiter().GetResult();
#endif
        }

        public void Dispose() => worker?.Dispose();

        /* PUBLIC API */

        public float[] Embed(string text)
        {
            try
            {
                var tokens = Tokenize(text);
                
                // WebGL safety: Check if we have reasonable token count
                if (tokens.Count > 512)
                {
                    Debug.LogWarning($"[StreamingAssets Embedder] Text too long for WebGL ({tokens.Count} tokens), truncating to 512");
                    tokens = tokens.Take(512).ToList();
                }
                
                // Create embedding tensor and ensure it's disposed
                var embedTensor = EmbedTokens(tokens);
                try
                {
                    // WebGL safety: Ensure tensor download doesn't cause memory issues
                    var result = embedTensor.DownloadToNativeArray().ToArray(); // [1, FEATURES]
                    
                    // Force garbage collection on WebGL to prevent memory buildup
#if UNITY_WEBGL && !UNITY_EDITOR
                    System.GC.Collect();
#endif
                    
                    return result;
                }
                finally
                {
                    // Explicitly dispose the embedding tensor
                    embedTensor?.Dispose();
                }
            }
            catch (System.OutOfMemoryException ex)
            {
                Debug.LogError($"[StreamingAssets Embedder] Out of memory during embedding (WebGL): {ex.Message}");
                throw new InvalidOperationException("WebGL memory limit exceeded during text embedding");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StreamingAssets Embedder] Embedding failed: {ex.Message}");
                throw;
            }
        }

        public void EmbedInPlace(System.Collections.Generic.List<RagChunk> chunks)
        {
            try
            {
               
                for (int i = 0; i < chunks.Count; i++)
                {
                    try
                    {
                        chunks[i].Embedding = Embed(chunks[i].Text);
                        
                        // WebGL: Progress logging and memory management
                        if ((i + 1) % 10 == 0 || i == chunks.Count - 1)
                        {                           
#if UNITY_WEBGL && !UNITY_EDITOR
                            // Force periodic garbage collection on WebGL
                            if ((i + 1) % 50 == 0)
                            {
                                System.GC.Collect();
                            }
#endif
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[StreamingAssets Embedder] Failed to embed chunk {i}: {ex.Message}");
                        throw new InvalidOperationException($"Embedding failed at chunk {i}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StreamingAssets Embedder] Batch embedding failed: {ex.Message}");
                throw;
            }
        }

        public int TokenizeCount(string text) => Tokenize(text).Count;

        /* PRIVATE IMPLEMENTATION - Same as original MiniLMEmbedder */

        // Tokenizer (greedy WordPiece)
        public List<int> Tokenize(string text)
        {
            var ids = new List<int> { START_TOKEN };
            var words = text.ToLower().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                int start = 0;
                while (start < word.Length)
                {
                    int end = word.Length;
                    int found = -1;

                    while (end > start)
                    {
                        string sub = (start == 0)
                            ? word.Substring(start, end - start)
                            : "##" + word.Substring(start, end - start);
                        int idx = Array.IndexOf(vocabTokens, sub);
                        if (idx >= 0) { found = idx; break; }
                        end--;
                    }

                    if (found == -1)
                    {
                        found = Array.IndexOf(vocabTokens, "[UNK]");
                        start = word.Length; // consume rest
                    }
                    else start = end;

                    ids.Add(found);
                }
            }

            ids.Add(END_TOKEN);
            return ids;
        }

        private Tensor<float> EmbedTokens(List<int> tokenList)
        {
            int N = tokenList.Count;
            using var input_ids = new Tensor<int>(new TensorShape(1, N), tokenList.ToArray());
            using var token_type_ids = new Tensor<int>(new TensorShape(1, N), new int[N]);
            var maskArr = Enumerable.Repeat(1, N).ToArray();
            using var attention_mask = new Tensor<int>(new TensorShape(1, N), maskArr);

            // Execute the model
            worker.Schedule(input_ids, attention_mask, token_type_ids);
            
            // Get output and ensure proper disposal
            var workerOutput = worker.PeekOutput();
            try
            {
                var output = workerOutput.ReadbackAndClone() as Tensor<float>; // [1, FEATURES]
                return output; // caller disposes
            }
            finally
            {
                // Explicitly dispose the worker output tensor
                workerOutput?.Dispose();
            }
        }

        /* Functional helpers */

        private static FunctionalTensor MeanPool(FunctionalTensor tokenEmb, FunctionalTensor attnMask, int features)
        {
            // tokenEmb: [1, seq, F], attnMask: [1, seq]
            var mask = attnMask.Unsqueeze(-1).BroadcastTo(new[] { features });               // [1, seq, F]
            var sum = Functional.ReduceSum(tokenEmb * mask, 1);                              // [1, F]
            var denom = Functional.ReduceSum(mask, 1) + 1e-9f;                               // [1, F]
            var mean = sum / denom;                                                          // [1, F]
            return mean;
        }

        private static FunctionalTensor L2Normalize(FunctionalTensor v)
        {
            // Calculate norm = sqrt(sum(v², axis=1)), then broadcast back to shape
            var squared = Functional.Square(v);                     // shape: [1, F]
            var sum = Functional.ReduceSum(squared, 1);             // shape: [1]
            var norm = Functional.Sqrt(sum).Unsqueeze(-1);          // shape: [1, 1]
            return v / (norm + 1e-9f);
        }
    }
}