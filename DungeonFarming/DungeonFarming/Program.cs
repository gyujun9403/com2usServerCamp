using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.Middleware;
using ZLogger;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.PurchaseDb;

var builder = WebApplication.CreateBuilder(args);

//builder.Host.ConfigureLogging();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddZLoggerFile("MainLog.log", options => { options.EnableStructuredLogging = true; });
    logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log", x => x.ToLocalTime().Date, 1024);
    logging.AddZLoggerConsole(options => { options.EnableStructuredLogging = true; });
});
builder.Services.AddControllers();
builder.Services.AddTransient<IAccountDb, MysqlAccountDb>();
builder.Services.AddTransient<IGameDb, MysqlGameDb>();
builder.Services.AddTransient<IPurchaseDb, MysqlPurchaseDb>();
builder.Services.AddSingleton<IGameSessionDb, RedisGameSessionDb>();
builder.Services.AddSingleton<IMasterDataOffer, MasterDataOffer>();

var app = builder.Build();
var masterDataOffer = app.Services.GetRequiredService<IMasterDataOffer>();
masterDataOffer.LoadMasterDatas();
app.MapControllers();
app.UseMiddleware<VersionCheckMiddleware>();
app.UseMiddleware<AuthCheckMiddleware>();

app.Run();
