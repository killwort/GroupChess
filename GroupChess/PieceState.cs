using System.Linq;
using ChessDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GroupChess
{
    public class PieceState
    {
        public PieceState((int col, int row, Piece piece) pieceOnBoard, ChessGame game)
        {
            Kind = pieceOnBoard.piece.GetFenCharacter().ToString();
            Player = pieceOnBoard.piece.Owner;
            Position = $"{(char)('a' + pieceOnBoard.col)}{8-pieceOnBoard.row}";
            PossibleMoves = pieceOnBoard.piece.GetValidMoves(new Position(Position), false, game, game.IsValidMove).Select(x => x.NewPosition.ToString()).ToArray();
        }

        public string Kind;

        [JsonConverter(typeof(StringEnumConverter))]
        public Player Player;

        public string Position;
        public string[] PossibleMoves;
    }
}