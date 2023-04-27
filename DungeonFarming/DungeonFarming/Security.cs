using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;

namespace DungeonFarming
{
    public static class Security
    {
        static readonly Int16 RepeatCnt = 10000;
        public static void Hashing(String rawPassword, out byte[] saltBytes, out byte[] hashedPasswordBytes)
        {
            saltBytes = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            // https://learn.microsoft.com/ko-kr/dotnet/api/system.security.cryptography.rfc2898derivebytes?view=net-7.0
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(rawPassword, saltBytes, RepeatCnt, HashAlgorithmName.SHA256);
            hashedPasswordBytes = pbkdf2.GetBytes(32);
        }

        public static bool VerifyHashedPassword(String rawPassword, byte[] saltBytes, byte[] hashedPasswordBytes)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(rawPassword, saltBytes, RepeatCnt, HashAlgorithmName.SHA256);
            byte[] newHashed = pbkdf2.GetBytes(32);
            return hashedPasswordBytes.SequenceEqual(newHashed);
        }

        public static String GenerateToken(String account_id)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(account_id + DateTime.Now.ToString()));
        }
    }
}
