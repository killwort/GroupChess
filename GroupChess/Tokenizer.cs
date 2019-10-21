using System.Collections.Generic;
using System.IO;

namespace GroupChess {
    class Tokenizer {
        private readonly TextReader _reader;

        public Tokenizer(TextReader reader) { _reader = reader; }

        public Tokenizer(BinaryReader reader) {
            var n = reader.ReadInt32();
            tokenValues = new List<string>(n);
            for (var i = 0; i < n; i++) {
                tokenValues.Add(reader.ReadString());
            }
        }

        public void Serialize(BinaryWriter writer) {
            writer.Write(tokenValues.Count);
            foreach (var t in tokenValues)
                writer.Write(t);
        }

        private Dictionary<string, int> tokens = new Dictionary<string, int>();
        private List<string> tokenValues = new List<string>();
        private int nextId = 0;

        protected int Token(string tokenValue) {
            if (!tokens.TryGetValue(tokenValue.ToLower(), out var id)) {
                tokens[tokenValue.ToLower()] = id = nextId++;
                tokenValues.Add(tokenValue.ToLower());
            }

            return id;
        }

        public string TokenValue(int id) {
            if (id == -1) return "END-OF-SEQUENCE";
            if (id == -2) return "END-OF-FILE";
            return tokenValues[id];
        }

        protected int Eos() => -1;

        public IEnumerable<int> Tokenize() {
            string accumulated = "";
            while (true) {
                var ch = _reader.Read();
                if (ch == -1 || char.IsWhiteSpace((char) ch) || (char.IsPunctuation((char) ch) && ch != '-')) {
                    if (accumulated.Length > 0)
                        yield return Token(accumulated);
                    if (ch == '.' || ch == '!' || ch == '?')
                        yield return Eos();
                    accumulated = "";
                    if (ch == -1)
                        yield break;
                }

                if (char.IsLetter((char) ch) || (ch == '-' && accumulated.Length > 0))
                    accumulated += (char) ch;
            }
        }
    }
}