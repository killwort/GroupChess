using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GroupChess {
    class MarkovChain {
        private readonly Tokenizer _tk;
        private readonly int _nGramLength;

        class ArrayComparer : IEqualityComparer<int[]> {
            public bool Equals(int[] x, int[] y) => x != null && y != null && x.Length == y.Length && x.Select((item, index) => y[index] == item).All(z => z);
            public int GetHashCode(int[] obj) => obj.Aggregate(0, (a, b) => a ^ b.GetHashCode());
        }

        private Dictionary<int[], Dictionary<int, int>> _nGrams = new Dictionary<int[], Dictionary<int, int>>(new ArrayComparer());

        public MarkovChain(Stream stream) {
            using (var reader = new BinaryReader(stream)) {
                _nGramLength = reader.ReadInt32();
                _tk = new Tokenizer(reader);
                var ngrams = reader.ReadInt32();
                for (var n = 0; n < ngrams; n++) {
                    var prefix = new int[_nGramLength];
                    for (var p = 0; p < _nGramLength; p++)
                        prefix[p] = reader.ReadInt32();
                    var suffixes = reader.ReadInt32();
                    var sss = new Dictionary<int, int>();
                    for (var s = 0; s < suffixes; s++) {
                        sss[reader.ReadInt32()] = reader.ReadInt32();
                    }

                    _nGrams[prefix] = sss;
                }
            }
        }

        public void Serialize(Stream stream) {
            using (var writer = new BinaryWriter(stream)) {
                writer.Write(_nGramLength);
                _tk.Serialize(writer);
                writer.Write(_nGrams.Count);
                foreach (var ngram in _nGrams) {
                    foreach (var c in ngram.Key)
                        writer.Write(c);
                    writer.Write(ngram.Value.Count);
                    foreach (var suffix in ngram.Value) {
                        writer.Write(suffix.Key);
                        writer.Write(suffix.Value);
                    }
                }
            }
        }

        public MarkovChain(Tokenizer tk, int nGramLength) {
            _tk = tk;
            _nGramLength = nGramLength;
            var prefix = new int[nGramLength];
            for (var i = 0; i < prefix.Length; i++) prefix[i] = -1;
            foreach (var token in tk.Tokenize()) {
                if (!_nGrams.TryGetValue(prefix, out var suffix))
                    _nGrams[prefix] = suffix = new Dictionary<int, int>();
                suffix.TryGetValue(token, out var freq);
                suffix[token] = freq + 1;
                var newPrefix = new int[prefix.Length];
                if (token == -1)
                    for (var i = 0; i < prefix.Length; i++)
                        newPrefix[i] = -1;
                else {
                    for (var k = 1; k < nGramLength; k++)
                        newPrefix[k - 1] = prefix[k];
                    newPrefix[nGramLength - 1] = token;
                }

                prefix = newPrefix;
            }
        }

        public IEnumerable<string> Value(Func<int, int> entropy, int softMaxLen, int hardMaxLen) {
            var prefix = new int[_nGramLength];
            for (var i = 0; i < prefix.Length; i++) prefix[i] = -1;
            while (hardMaxLen-- > 0) {
                if (!_nGrams.TryGetValue(prefix, out var suffixes) || suffixes == null || suffixes.Count == 0) {
                    yield break;
                }

                if (--softMaxLen <= 0 && suffixes.ContainsKey(-1))
                    yield break;
                var suffixesTotal = suffixes.Sum(x => x.Value);
                var targetSuffix = entropy(suffixesTotal);
                var next = suffixes.SkipWhile(x => (targetSuffix -= x.Value) > 0).First();
                if (next.Key == -1) {
                    yield break;
                }

                for (var k = 1; k < _nGramLength; k++)
                    prefix[k - 1] = prefix[k];
                prefix[_nGramLength - 1] = next.Key;
                yield return _tk.TokenValue(next.Key);
            }
        }
    }
}