using System.Text;
using System.Text.Json;

namespace DungeonFarming.Middleware
{
    public class VersionCheckMiddleware
    {
        readonly RequestDelegate _next;
        readonly String _clientVersion;
        readonly String _masterDataVersion;

        public VersionCheckMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _clientVersion = config.GetSection("Versions")["Client"];
            _masterDataVersion = config.GetSection("Versions")["Master_Data"];
        }
        public async Task Invoke(HttpContext context)
        {
            String path = context.Request.Path;
            if (!(path.StartsWith("/Registe")))
            {
                context.Request.EnableBuffering();
                using (var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    var requestBody = await streamReader.ReadToEndAsync();
                    if (await CheckVersions(context, requestBody) == false)
                    {
                        return ;
                    }
                }
                context.Request.Body.Position = 0;
            }
            await _next(context);
        }

        private async Task<bool> CheckVersions(HttpContext context, String? requestBody)
        {
            if (requestBody == null)
            {
                await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                return false;
            }
            try
            {
                var doc = JsonDocument.Parse(requestBody);
                if (doc == null)
                {
                    // TODO: Logger
                    await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                    return false;
                }
                if (doc.RootElement.TryGetProperty("clientVersion", out var clientVersion) == false
                    || doc.RootElement.TryGetProperty("masterDataVersion", out var masterDataVersion) == false)
                {
                    //
                    await SetContext(context, 400, ErrorCode.InvalidBodyForm);
                    return false;
                }
                if (clientVersion.ValueEquals(_clientVersion) == false)
                {
                    await SetContext(context, 400, ErrorCode.WorngClientVersion);
                    return false;
                }
                if (masterDataVersion.ValueEquals(_masterDataVersion) == false)
                {
                    await SetContext(context, 400, ErrorCode.WorngMasterDataVersion);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                await SetContext(context, 500, ErrorCode.ServerError);
                return false;
            }
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
