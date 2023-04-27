using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using MySqlConnector;
using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddTransient<IAccountDb, MysqlAccountDb>();

var app = builder.Build();
app.MapControllers();
app.UseMiddleware<AuthCheckMiddleware>();

app.Run();
