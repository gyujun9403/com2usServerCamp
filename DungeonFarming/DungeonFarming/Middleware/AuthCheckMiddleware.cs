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
        public async Task Invoke(HttpContext context)
        {
            //요청 로직
            String path = context.Request.Path;
            if (!(path.StartsWith("/Registe") || path.StartsWith("/Login")))
            {
                //_gameSessionDb.GetUserInfoSession()
            }
            await _next(context);
            //응답 로직
        }
    }
}
