﻿using DungeonFarming.DataBase.GameSessionDb;
using System.Text;
using System.Text.Json;
using ZLogger;

namespace DungeonFarming.Middleware;

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
        if (path.StartsWith("/Regist") || path.StartsWith("/Login"))
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();
        using var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);
        var requestBody = await streamReader.ReadToEndAsync();
        if (String.IsNullOrEmpty(requestBody))
        {
            _logger.ZLogWarningWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path }, "Http Body NULLorEMPTY");
            await SetContext(context, 400, ErrorCode.InvalidBodyForm);
            return ;
        }

        var (userAssignedId, requestToken) = await GetIdToken(context, requestBody);
        if (userAssignedId == null || requestToken == null)
        {
            return;
        }

        var userSession = await GetUserSession(userAssignedId);
        if (userSession == null)
        {
            return;
        }
        if (userSession.token != requestToken)
        {
            await SetContext(context, 400, ErrorCode.InvalidToken);
            return ;
        }

        context.Items["userSession"] = userSession;
        context.Request.Body.Position = 0;
        
        await _next(context);
    }

    private async Task<(String?, String?)> GetIdToken(HttpContext context, String requestBody)
    {
        try
        {
            var doc = JsonDocument.Parse(requestBody);
            if (doc == null)
            {
                _logger.ZLogWarningWithPayload(LogEventId.AuthCheck, new { Path = context.Request.Path }, "Http Body UNPARSINGABLE");
                await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                return (null, null);
            }
            else if (doc.RootElement.TryGetProperty("userAssignedId", out var id))
            {
                if (doc.RootElement.TryGetProperty("token", out var token))
                {
                    return (id.GetString(), token.GetString());
                }
            }
            return (null, null);
        }
        catch (Exception ex) 
        {
            _logger.ZLogWarningWithPayload(LogEventId.AuthCheck, ex, new { Path = context.Request.Path }, "Http ");
            await SetContext(context, 500, ErrorCode.ServerError);
            return (null, null);
        }
    }

    async Task<GameSessionData?> GetUserSession(String userAssignedId)
    {
        var (errorCode, userInfo) = await _gameSessionDb.GetUserInfoSession(userAssignedId);
        if (errorCode != ErrorCode.None || userInfo == null)
        {
            return null;
        }
        return userInfo;
    }

    private async Task SetContext(HttpContext context, Int32 statusCode, ErrorCode errorCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var responseContent = new { error_code = errorCode };
        await context.Response.WriteAsJsonAsync(responseContent);
    }


}
