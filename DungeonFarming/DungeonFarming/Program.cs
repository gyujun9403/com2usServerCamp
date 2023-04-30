using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using MySqlConnector;
using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.Middleware;
using ZLogger;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DataBase.GameDb;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddZLoggerConsole();
    logging.AddZLoggerFile("TestLogFile.log");
    logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024);
    logging.AddZLoggerConsole(options => { options.EnableStructuredLogging = true; });
});
builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddTransient<IAccountDb, MysqlAccountDb>();
builder.Services.AddTransient<IGameDb, MysqlGameDb>();
builder.Services.AddSingleton<IGameSessionDb, RedisGameSessionDb>();
var app = builder.Build();
app.MapControllers();
app.UseMiddleware<AuthCheckMiddleware>();

app.Run();
