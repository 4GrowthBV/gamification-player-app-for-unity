using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.InferenceEngine;

namespace GamificationPlayer
{
    public class MiniLMEmbedder : IDisposable
    {
        private readonly int FEATURES;
        private readonly Worker worker;
        private readonly string[] vocabTokens;

        // model expects 3 inputs: input_ids, attention_mask, token_type_ids
        // output: "last_hidden_state" [1, seq, FEATURES]
        private const int START_TOKEN = 101;
        private const int END_TOKEN = 102;

        public MiniLMEmbedder(ModelAsset modelAsset, TextAsset vocabAsset, BackendType backend, int features)
        {
            FEATURES = features;

            // Load vocab (robust line splitting)
            vocabTokens = vocabAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Compose model graph that adds masked mean pool + L2 normalize to output
            var baseModel = ModelLoader.Load(modelAsset);
            var g = new FunctionalGraph();
            var inputs = g.AddInputs(baseModel);                         // input_ids, attention_mask, token_type_ids
            var lastHidden = Functional.Forward(baseModel, inputs)[0];   // "last_hidden_state"
            var attentionMask = inputs[1];

            var pooled = MeanPool(lastHidden, attentionMask, FEATURES);
            var normalized = L2Normalize(pooled);
            var compiled = g.Compile(normalized);

            worker = new Worker(compiled, backend);
        }

        public void Dispose() => worker?.Dispose();

        /* PUBLIC API */

        public float[] Embed(string text)
        {
            var tokens = Tokenize(text);
            using var t = EmbedTokens(tokens);
            return t.DownloadToNativeArray().ToArray(); // [1, FEATURES]
        }

        public void EmbedInPlace(List<RagChunk> chunks)
        {
            // Single engine, reuse buffers per chunk length
            foreach (var c in chunks)
            {
                var ids = Tokenize(c.Text);
                using var t = EmbedTokens(ids);
                c.Embedding = t.DownloadToNativeArray().ToArray();
            }
        }

        public int TokenizeCount(string text) => Tokenize(text).Count;

        /* CORE */

        private Tensor<float> EmbedTokens(List<int> tokenList)
        {
            int N = tokenList.Count;
            using var input_ids = new Tensor<int>(new TensorShape(1, N), tokenList.ToArray());
            using var token_type_ids = new Tensor<int>(new TensorShape(1, N), new int[N]);
            var maskArr = Enumerable.Repeat(1, N).ToArray();
            using var attention_mask = new Tensor<int>(new TensorShape(1, N), maskArr);

            worker.Schedule(input_ids, attention_mask, token_type_ids);
            var output = worker.PeekOutput().ReadbackAndClone() as Tensor<float>; // [1, FEATURES]
            return output; // caller disposes
        }

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