using DungeonFarming.DataBase.GameSessionDb;
using System.Text;
using System.Text.Json;
using ZLogger;

namespace DungeonFarming.Middleware
{
    public class AuthCheckMiddleware
    {
        readonly RequestDelegate _next;
        readonly IGameSessionDb _gameSessionDb;
        readonly ILogger<AuthCheckMiddleware> _logger;

        public AuthCheckMiddleware(RequestDelegate next, IGameSessionDb gameSessionDb, ILogger<AuthCheckMiddleware> logger)
        {
            _next = next;
            _gameSessionDb = gameSessionDb;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            String path = context.Request.Path;
            if (!(path.StartsWith("/Regist") || path.StartsWith("/Login")))
            {
                context.Request.EnableBuffering();
                using (var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var requestBody = await streamReader.ReadToEndAsync(); // 요청 본문을 문자열로 읽어옵니다.
                    // 요청 body에 id, token이 있는지 확인하고, 있는 경우 찾아서 반환.
                    var (userId, token) = await CheckBodyFormAndGetIdToken(context, requestBody);

                    if (userId == null || token == null)
                    {
                        return;
                    }
                    if (await CheckTokenAndSetHeader(context, userId, token) == false)
                    {
                        return;
                    }
                }
                context.Request.Body.Position = 0;
            }
            await _next(context);
            //응답 로직
        }

        private async Task<(String?, String?)> CheckBodyFormAndGetIdToken(HttpContext context, String? body)
        {
            if (String.IsNullOrEmpty(body))
            {
                _logger.ZLogWarningWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path }, "Http Body NULLorEMPTY");
                await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                return (null, null);
            }
            try
            {
                var doc = JsonDocument.Parse(body);
                if (doc == null)
                {
                    _logger.ZLogWarningWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path }, "Http Body UNPARSINGABLE");
                    await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                    return (null, null);
                }
                else if (doc.RootElement.TryGetProperty("userId", out var id))
                {
                    if (doc.RootElement.TryGetProperty("token", out var token))
                    {
                        return (id.GetString(), token.GetString());
                    }
                }
                return (null, null);
            }
            catch(FormatException ex)
            {
                _logger.ZLogWarningWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path, Exception = ex }, "Http Body FormatEXCEPTION");
                await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                return (null, null);
            }
            catch
            {
                _logger.ZLogErrorWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path }, "Http SERVERERROR");
                await SetContext(context, 500, ErrorCode.ServerError);
                return (null, null);
            }
        }

        private async Task<bool> CheckTokenAndSetHeader(HttpContext context, String userId, String inputToken)
        {
            var (errorCode, userInfo) = await _gameSessionDb.GetUserInfoSession(userId);
            if (errorCode != ErrorCode.None || userInfo == null)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path, ErrorCode = errorCode }, "Session FAIL");
                await SetContext(context, 400, errorCode);
                return false;
            }
            if (userInfo.token != inputToken)
            {
                _logger.ZLogInformationWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path, userId = userId }, "Token Check FAIL");
                await SetContext(context, 400, ErrorCode.InvalidToken);
                return false;
            }
            context.Request.Headers.Add("UserPkId", userInfo.pkId.ToString());
            return true;
        }

        private async Task SetContext(HttpContext context, Int32 statusCode, ErrorCode errorCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var responseContent = new { error_code = errorCode };
            await context.Response.WriteAsJsonAsync(responseContent);
        }


    }
}
