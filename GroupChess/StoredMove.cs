using System;

namespace GroupChess
{
    public class StoredMove
    {
        public DateTime Timestamp = DateTime.UtcNow;
        public string FromPosition;
        public string ToPosition;
        public string Author;
        public string Promotion;
    }
}