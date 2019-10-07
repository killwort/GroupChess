using System;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using ChessDotNet;
using FB.Utils;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using File = System.IO.File;

namespace GroupChess
{
    public class GameStore
    {
        private readonly IPathResolver _resolver;
        private readonly IMemoryCache _memoryCache;

        public GameStore(IPathResolver resolver, IMemoryCache memoryCache)
        {
            _resolver = resolver;
            _memoryCache = memoryCache;
        }

        public async Task<GameState> NewGame()
        {
            var id = Guid.NewGuid().ToString("N");
            _memoryCache.CreateEntry("Game-" + id);
            var state = new GameState(id, new ChessGame(), new StoredMove[0])
            {
                State = GameStates.InProgress,
                Started = DateTime.UtcNow,
                Finished = DateTime.UtcNow,
                LastMove = DateTime.UtcNow
            };
            await SaveGame(state);
            return state;
        }

        private async Task SaveGame(GameState game)
        {
            var gameInfo = _resolver.Resolve($"games/{game.GameId}.json");
            if (!Directory.Exists(Path.GetDirectoryName(gameInfo.FullName)))
                Directory.CreateDirectory(Path.GetDirectoryName(gameInfo.FullName));
            await File.WriteAllTextAsync(gameInfo.FullName, JsonConvert.SerializeObject(game.StorableMoves));
        }

        public Task<GameState> GetGame(string id) =>
            _memoryCache.GetOrCreateAsync("Game-" + id, async ce =>
            {
                var gameInfo = _resolver.Resolve($"games/{id}.json");
                if (!Path.GetFullPath(gameInfo.FullName).StartsWith(Path.GetFullPath(_resolver.Resolve("games").FullName)))
                    return null;
                if (!gameInfo.Exists)
                {
                    ce.SlidingExpiration = TimeSpan.FromMinutes(1);
                    return null;
                }

                var moves = JsonConvert.DeserializeObject<StoredMove[]>(await File.ReadAllTextAsync(gameInfo.FullName));
                var plr = Player.Black;
                var game = moves.Length == 0
                    ? new ChessGame()
                    : new ChessGame(moves.Select(x =>
                    {
                        plr = (Player) (((int) plr + 1) % 2);
                        return new Move(x.FromPosition, x.ToPosition, plr, string.IsNullOrEmpty(x.Promotion) ? null : (char?) x.Promotion[0]);
                    }), true);
                var rv= new GameState(id, game, moves);
                rv.Started = gameInfo.CreationTimeUtc;
                rv.CalculateState();
                return rv;
            });

        public Task<string[]> ListGames(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) prefix = "";
            var gameInfo = _resolver.Resolve($"games/{prefix}".Trim('/'));
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('/'))
                prefix = prefix + '/';
            if (!Path.GetFullPath(gameInfo.FullName).StartsWith(Path.GetFullPath(_resolver.Resolve("games").FullName)))
                return Task.FromResult(new string[0]);
            if (!Directory.Exists(gameInfo.FullName))
                return Task.FromResult(new string[0]);
            return Task.FromResult(Directory.GetFiles(gameInfo.FullName, "*.json", SearchOption.TopDirectoryOnly)
                .Select(x => prefix + Path.GetFileNameWithoutExtension(x)).ToArray());
        }

        public Task Save(GameState gameState)=>SaveGame(gameState);
    }
}