using System;

namespace GroupChess
{
    public class StoredMove
    {
        public DateTime Timestamp = DateTime.UtcNow;
        public string MovedPiece;
        public string TakenPiece;
        public bool Check;
        public string FromPosition;
        public string ToPosition;
        public string Author;
        public string Promotion;
    }
}