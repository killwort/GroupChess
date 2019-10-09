using System;
using System.Globalization;
using System.Linq;
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
            {
                Response.StatusCode = 404;
                return null;
            }

            if (Request.Headers["If-Modified-Since"].Any())
            {
                var ims = DateTime.ParseExact(Request.Headers["If-Modified-Since"], "R", CultureInfo.InvariantCulture);
                if (ims >= g.LastMove.AddMilliseconds(-g.LastMove.Millisecond-1))
                {
                    Response.StatusCode = 304;
                    return null;
                }
            }
            return g;
        }

        [HttpDelete("game/{id}")]
        public async Task DeleteGame([FromRoute] string id) {
            var g = await _gameStore.GetGame(id);
            if (g == null)
            {
                Response.StatusCode = 404;
                return;
            }

            await _gameStore.DeleteGame(g);
        }

        [HttpPost("game/{id}/move/{from}/{to}/{promotion?}")]
        public async Task<GameState> MakeMove([FromRoute] string id, [FromRoute]string from,[FromRoute] string to, [FromBody]string who, [FromRoute] string promotion)
        {
            var g = await _gameStore.GetGame(id);
            if (string.IsNullOrEmpty(who) || who.Length > 50)
                who = "Анонимный Анонимщик";
            if (g == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            g.MakeMove(from, to, who, promotion);
            await _gameStore.Save(g);
            return g;
        }

        [HttpGet("game")]
        public Task<string[]> ListGames(string prefix) => _gameStore.ListGames(prefix);

        [HttpPost("game")]
        public async Task<string> NewGame()
        {
            var g = await _gameStore.NewGame();
            return g.GameId;
        }
    }
}