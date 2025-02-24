using System.Security.Cryptography;

namespace backend_server_mvc.Util
{
    public class TokenGenerator
    {
        public static string GenerateToken(int length = 32)
        {
            byte[] tokenBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes);
        }
    }

}
