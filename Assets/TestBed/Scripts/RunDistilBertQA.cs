using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity.InferenceEngine;
using Unity.Collections;

public class RunDistilBertQA : MonoBehaviour
{
    [Header("Model + Vocab")]
    public ModelAsset modelAsset;      // model.onnx
    public TextAsset vocabAsset;       // vocab.txt
    const BackendType backend = BackendType.CPU;

    [Header("Input")]
    [TextArea(2, 4)] public string question = "Wie schreef het boek Max Havelaar?";
    [TextArea(4, 10)] public string context = "Max Havelaar werd geschreven door Multatuli. Het boek verscheen in 1860 en is een aanklacht tegen de wantoestanden in Nederlands-Indië.";

    const int START_TOKEN = 101;
    const int SEP_TOKEN = 102;
    const int PAD_TOKEN = 0;
    const int UNK_TOKEN = 100;
    const int MAX_SEQ_LEN = 384;

    string[] vocab;
    Worker qaWorker;

    // --- Helper structs ---
    class QAResult
    {
        public float score;
        public int start;
        public int end;
        public string answer;
    }

    struct TokenInfo
    {
        public int id;
        public int startChar;
        public int endChar;
        public string text;
    }

    void Start()
    {
        vocab = vocabAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        qaWorker = CreateQAWorker();

        var (inputIds, attentionMask, tokenInfos) = BuildInput(question, context);
        qaWorker.Schedule(inputIds, attentionMask);

        var startLogits = qaWorker.PeekOutput("start_logits") as Tensor<float>;
        var endLogits = qaWorker.PeekOutput("end_logits") as Tensor<float>;

        var start = startLogits.DownloadToNativeArray();
        var end = endLogits.DownloadToNativeArray();

        var result = DecodeAnswer(inputIds, tokenInfos, start, end);

        Debug.Log($"{{'score': {result.score}, 'start': {result.start}, 'end': {result.end}, 'answer': '{result.answer}'}}");

        inputIds.Dispose();
        attentionMask.Dispose();
        startLogits.Dispose();
        endLogits.Dispose();
    }

    Worker CreateQAWorker()
    {
        var model = ModelLoader.Load(modelAsset);
        return new Worker(model, backend);
    }

    (Tensor<int>, Tensor<int>, List<TokenInfo>) BuildInput(string question, string context)
    {
        var q = Tokenize(question, trackOffsets: false);
        var c = Tokenize(context, trackOffsets: true);

        var tokens = new List<int>();
        var tokenInfos = new List<TokenInfo>();

        // [CLS] Q [SEP] C [SEP]
        tokens.Add(START_TOKEN);
        tokenInfos.Add(new TokenInfo { id = START_TOKEN, text = "[CLS]" });

        foreach (var t in q)
        {
            tokens.Add(t.id);
            tokenInfos.Add(t);
        }

        tokens.Add(SEP_TOKEN);
        tokenInfos.Add(new TokenInfo { id = SEP_TOKEN, text = "[SEP]" });

        foreach (var t in c)
        {
            tokens.Add(t.id);
            tokenInfos.Add(t);
        }

        tokens.Add(SEP_TOKEN);
        tokenInfos.Add(new TokenInfo { id = SEP_TOKEN, text = "[SEP]" });

        if (tokens.Count > MAX_SEQ_LEN)
        {
            tokens = tokens.Take(MAX_SEQ_LEN).ToList();
            tokenInfos = tokenInfos.Take(MAX_SEQ_LEN).ToList();
        }

        var attn = Enumerable.Repeat(1, tokens.Count).ToList();
        while (tokens.Count < MAX_SEQ_LEN)
        {
            tokens.Add(PAD_TOKEN);
            attn.Add(0);
            tokenInfos.Add(new TokenInfo { id = PAD_TOKEN, text = "[PAD]" });
        }

        var inputIds = new Tensor<int>(new TensorShape(1, MAX_SEQ_LEN), tokens.ToArray());
        var attentionMask = new Tensor<int>(new TensorShape(1, MAX_SEQ_LEN), attn.ToArray());

        return (inputIds, attentionMask, tokenInfos);
    }

    // ---- WordPiece Tokenizer with optional char offsets ----
    List<TokenInfo> Tokenize(string text, bool trackOffsets)
    {
        var result = new List<TokenInfo>();
        int length = text.Length;
        int i = 0;

        while (i < length)
        {
            if (char.IsWhiteSpace(text[i])) { i++; continue; }

            int start = i;
            while (i < length && !char.IsWhiteSpace(text[i])) i++;
            int end = i;
            string word = text.Substring(start, end - start);
            int wordStart = start;

            int pos = 0;
            while (pos < word.Length)
            {
                int subEnd = word.Length;
                int foundId = -1;
                string sub = "";

                while (pos < subEnd)
                {
                    string candidate = word.Substring(pos, subEnd - pos);
                    if (pos > 0) candidate = "##" + candidate;

                    int idx = Array.IndexOf(vocab, candidate);
                    if (idx != -1)
                    {
                        foundId = idx;
                        sub = candidate;
                        break;
                    }
                    subEnd--;
                }

                if (foundId == -1)
                {
                    foundId = UNK_TOKEN;
                    result.Add(new TokenInfo
                    {
                        id = foundId,
                        startChar = trackOffsets ? wordStart + pos : 0,
                        endChar = trackOffsets ? wordStart + word.Length : 0,
                        text = "[UNK]"
                    });
                    break;
                }

                int tokenStartChar = wordStart + pos;
                int tokenEndChar = wordStart + (subEnd - (sub.StartsWith("##") ? 2 : 0));

                result.Add(new TokenInfo
                {
                    id = foundId,
                    startChar = trackOffsets ? tokenStartChar : 0,
                    endChar = trackOffsets ? tokenEndChar : 0,
                    text = sub
                });

                pos = subEnd;
            }
        }

        return result;
    }

    // ---- Decode answer and compute score ----
    QAResult DecodeAnswer(Tensor<int> inputIds, List<TokenInfo> tokens, NativeArray<float> startLogits, NativeArray<float> endLogits)
    {
        int len = inputIds.shape[1];
        int bestStart = 0, bestEnd = 0;
        float bestSum = float.NegativeInfinity;

        // Find best span by sum of logits
        for (int s = 0; s < len; s++)
        {
            for (int e = s; e < Math.Min(s + 30, len); e++)
            {
                float score = startLogits[s] + endLogits[e];
                if (score > bestSum)
                {
                    bestSum = score;
                    bestStart = s;
                    bestEnd = e;
                }
            }
        }

        // --- Joint softmax for Hugging Face–style confidence ---
        float maxSum = bestSum;
        float sumExp = 0f;
        for (int s = 0; s < len; s++)
        {
            for (int e = s; e < Math.Min(s + 30, len); e++)
                sumExp += Mathf.Exp(startLogits[s] + endLogits[e] - maxSum);
        }

        float confidence = Mathf.Exp(bestSum - maxSum) / sumExp;

        // --- Decode tokens to text ---
        var sb = new StringBuilder();
        for (int i = bestStart; i <= bestEnd && i < tokens.Count; i++)
        {
            string tok = tokens[i].text;
            if (tok == "[CLS]" || tok == "[SEP]" || tok == "[PAD]") continue;
            if (tok.StartsWith("##")) sb.Append(tok.Substring(2));
            else
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(tok);
            }
        }

        string answerText = sb.ToString().Trim();

        // --- Character offsets ---
        int charStart = 0, charEnd = 0;
        if (bestStart < tokens.Count && bestEnd < tokens.Count)
        {
            charStart = tokens[bestStart].startChar;
            charEnd = tokens[bestEnd].endChar;
        }

        return new QAResult
        {
            score = confidence,
            start = charStart,
            end = charEnd,
            answer = answerText
        };
    }

    void OnDestroy()
    {
        qaWorker?.Dispose();
    }
}
