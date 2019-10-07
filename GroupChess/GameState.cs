using System;
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
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public DateTime LastMove { get; set; }
        public GameStates State { get; set; }
        public IEnumerable<StoredMove> Moves => _moves;

        [JsonConverter(typeof(StringEnumConverter))]
        public Player CurrentPlayer => _chessGame.WhoseTurn;

        public PieceState[] Pieces =>
            _chessGame.GetBoard().SelectMany((colPieces, row) => colPieces.Select((piece, col) => (col, row, piece)))
                .Where(x => x.piece != null)
                .Select(x => new PieceState(x, _chessGame)).ToArray();

        public bool MakeMove(string @from, string to, string who, string promotion)
        {
            var mt = _chessGame.MakeMove(new Move(from, to, _chessGame.WhoseTurn, string.IsNullOrEmpty(promotion) ? null : (char?) promotion[0]), false);
            if ((mt & MoveType.Invalid) != 0) return false;
            _moves.Add(new StoredMove
            {
                FromPosition = from,
                ToPosition = to,
                Author = who,
                Promotion = promotion
            });
            CalculateState();
            return true;
        }

        public void CalculateState()
        {
            LastMove = _moves.Count == 0 ? Started : _moves.Last().Timestamp;
            if (_chessGame.IsCheckmated(Player.Black) || _chessGame.IsCheckmated(Player.White))
                State = GameStates.Checkmate;
            else if(_chessGame.IsStalemated(Player.Black)||_chessGame.IsStalemated(Player.White))
                State = GameStates.Stalemate;
            else if (_chessGame.IsDraw())
                State = GameStates.Draw;
            else State = GameStates.InProgress;
            if (State != GameStates.InProgress)
                Finished = LastMove;
        }
    }
}