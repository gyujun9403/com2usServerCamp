namespace DungeonFarming.Middleware
{
    public class AuthCheckMiddleware
    {
        private RequestDelegate _next;

        public AuthCheckMiddleware(RequestDelegate next)
        {
            _next = next;
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
