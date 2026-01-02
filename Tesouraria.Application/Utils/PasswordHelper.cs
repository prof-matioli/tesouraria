using System.Security.Cryptography;
using System.Text;

namespace Tesouraria.Application.Utils
{
    public static class PasswordHelper
    {
        public static string GenerateHash(string senha)
        {
            if (string.IsNullOrEmpty(senha)) return null;

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                var builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string senhaDigitada, string hashSalvo)
        {
            var hashDigitado = GenerateHash(senhaDigitada);
            return hashDigitado == hashSalvo;
        }
    }
}