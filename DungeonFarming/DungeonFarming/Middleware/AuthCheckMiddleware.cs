using DungeonFarming.DataBase.GameSessionDb;

namespace DungeonFarming.Middleware
{
    public class AuthCheckMiddleware
    {
        private RequestDelegate _next;
        private IGameSessionDb _gameSessionDb;

        public AuthCheckMiddleware(RequestDelegate next, IGameSessionDb gameSessionDb)
        {
            _next = next;
            _gameSessionDb = gameSessionDb;
        }
        public void Invoke(HttpContext context)
        {
            String path = context.Request.Path;
            if (!(path.StartsWith("/Registe") || path.StartsWith("/Login")))
            {
                // 토큰 검사 로직
            }
        }
    }
}
