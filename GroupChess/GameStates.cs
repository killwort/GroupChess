using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GroupChess
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameStates
    {
        InProgress,
        Checkmate,
        Stalemate,
        Draw
    }
}