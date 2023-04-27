using SqlKata.Execution;
using SqlKata.Compilers;
using MySqlConnector;

namespace DungeonFarming.DataBase.AccountDb
{
    public class MysqlAccountDb : IAccountDb
    {
        QueryFactory _db;
        public MysqlAccountDb(IConfiguration config)
        {
            String connString = config.GetConnectionString("Mysql_Account");
            var connection = new MySqlConnection(connString);
            var compiler = new MySqlCompiler();
            _db = new QueryFactory(connection, compiler);
        }
        public async Task<ErrorCode> RegisteUser(AccountDbModel model)
        {
            try
            {
                await _db.Query("account").InsertAsync(new
                {
                    user_id = model.account_id,
                    salt = model.salt,
                    hashed_password = model.hashed_password,
                });
            }
            catch (Exception ex)
            {
                // 중복인경우 ERROR_CODE.DUPLICATED_ID를 던지게 추가

            }
            return ErrorCode.ErrorNone;
        }
    }
}
