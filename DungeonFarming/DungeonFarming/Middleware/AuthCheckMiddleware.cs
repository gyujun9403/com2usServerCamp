using DungeonFarming.DataBase.GameSessionDb;
using System.Text;
using System.Text.Json;

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
            String path = context.Request.Path;
            // 등록 및 로그인 동작이 아닐 경우 하는 검사.
            if (!(path.StartsWith("/Registe") || path.StartsWith("/Login")))
            {
                context.Request.EnableBuffering(); // 요청 본문을 다시 읽을 수 있도록 EnableBuffering() 메소드를 호출합니다.
                using (var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var requestBody = await streamReader.ReadToEndAsync(); // 요청 본문을 문자열로 읽어옵니다.
                    //var jsonDoc = JsonDocument.Parse(requestBody); // 요청 본문을 JSON 객체로 파싱합니다.
                    // 요청 body에 id, token이 있는지 확인하고, 있는 경우 찾아서 반환.
                    var (userId, token) = await CheckBodyFormAndGetIdToken(context, requestBody);
                    if (userId == null || token == null)
                    {
                        return; // 잘못된 요청이므로 바로 응답으로 날린다.
                    }
                    // 토큰 확인
                    if (await CheckToken(context, userId, token) == false)
                    {
                        return;
                    }
                }
                context.Request.Body.Position = 0; // 요청 본문의 Position을 0으로 설정하여 다시 읽을 수 있도록 합니다.
            }
            await _next(context);
            //응답 로직
        }

        private async Task<(String?, String?)> CheckBodyFormAndGetIdToken(HttpContext context, String? body)
        {
            if (String.IsNullOrEmpty(body))
            {
                await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                return (null, null);
            }
            try
            {
                var doc = JsonDocument.Parse(body);
                if (doc == null)
                {
                    await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                    return (null, null);
                }
                else if (doc.RootElement.TryGetProperty("userId", out var id))
                {
                    if (doc.RootElement.TryGetProperty("token", out var token))
                        return (id.GetString(), token.GetString());
                }
                return (null, null);
            }
            catch(FormatException ex)
            {
                await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                return (null, null);
            }
            catch
            {
                await SetContext(context, 500, ErrorCode.ServerError);
                return (null, null);
            }
        }

        private async Task<bool> CheckToken(HttpContext context, String userId, String inputToken)
        {
            var (errorCode, userInfo) = await _gameSessionDb.GetUserInfoSession(userId);
            if (errorCode != ErrorCode.None || userInfo == null)
            {
                await SetContext(context, 400, errorCode);
                return false;
            }
            if (userInfo.token != inputToken)
            {
                await SetContext(context, 400, ErrorCode.InvalidToken);
                return false;
            }
            return true;
        }

        private async Task SetContext(HttpContext context, int statusCode, ErrorCode errorCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var responseContent = new { error_code = errorCode };
            await context.Response.WriteAsJsonAsync(responseContent);
        }
    }
}
