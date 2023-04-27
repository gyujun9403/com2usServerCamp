using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using MySqlConnector;
using DungeonFarming.DataBase.AccountDb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddTransient<QueryFactory>((e) =>
//{
//    var connection = new MySqlConnection(builder.Configuration.GetConnectionString("Mysql_Account"));
//    var compiler = new MySqlCompiler();
//    return new QueryFactory(connection, compiler);
//});
builder.Services.AddTransient<IAccountDb, MysqlAccountDb>();

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
