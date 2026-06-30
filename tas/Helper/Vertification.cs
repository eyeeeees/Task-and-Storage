using System.Security.Cryptography;
using System.Text;

namespace tas.Helpers
{
    public static class Vertification
    {
        public static string GetSHA512Hash(string input)
        {
            using SHA512 sha512 = SHA512.Create();
            byte[] data = sha512.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            foreach (byte b in data)
                sBuilder.Append(b.ToString("x2"));
            return sBuilder.ToString();
        }

        public static bool VerifySHA512Hash(string input, string hash)
        {
            string hashOfInput = GetSHA512Hash(input);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
        }
    }
}