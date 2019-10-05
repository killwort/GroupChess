using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GroupChess
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly GameStore _gameStore;

        public ApiController(GameStore gameStore)
        {
            _gameStore = gameStore;
        }

        [HttpGet("game/{id}")]
        public async Task<GameState> GetGame([FromRoute] string id)
        {
            var g = await _gameStore.GetGame(id);
            if (g == null)
                Response.StatusCode = 404;
            return g;
        }

        [HttpGet("game")]
        public async Task<string[]> ListGames(string prefix) => _gameStore.ListGames(prefix);

        [HttpPost("game")]
        public async Task<GameState> NewGame()
        {
            var g = await _gameStore.NewGame();
            return g;
        }
    }
}