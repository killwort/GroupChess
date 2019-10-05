using System.Collections.Generic;
using System.Linq;
using ChessDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GroupChess
{
    public class GameState
    {
        [JsonIgnore] private readonly ChessGame _chessGame;
        [JsonIgnore] private readonly List<StoredMove> _moves;
        [JsonIgnore] public IEnumerable<StoredMove> StorableMoves => _moves;

        public GameState()
        {
        }

        public GameState(string game, ChessGame chessGame, StoredMove[] moves)
        {
            GameId = game;
            _chessGame = chessGame;
            _moves = moves.ToList();
        }

        public string GameId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Player CurrentPlayer => _chessGame.WhoseTurn;

        public PieceState[] Pieces =>
            _chessGame.GetBoard().SelectMany((colPieces, row) => colPieces.Select((piece, col) => (col, row, piece)))
                .Where(x => x.piece != null)
                .Select(x => new PieceState(x, _chessGame)).ToArray();
    }

    public class PieceState
    {
        public PieceState((int col, int row, Piece piece) pieceOnBoard, ChessGame game)
        {
            Kind = pieceOnBoard.piece.GetFenCharacter().ToString();
            Player = pieceOnBoard.piece.Owner;
            Position = $"{(char)('a' + pieceOnBoard.col)}{pieceOnBoard.row+1}";
            PossibleMoves = pieceOnBoard.piece.GetValidMoves(new Position(Position), false, game, game.IsValidMove).Select(x => x.NewPosition.ToString()).ToArray();
        }

        public string Kind;

        [JsonConverter(typeof(StringEnumConverter))]
        public Player Player;

        public string Position;
        public string[] PossibleMoves;
    }
}