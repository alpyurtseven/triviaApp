using System;
namespace triviaApp.Utils
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class Token
    {
        private const string SecretKey = "K:1iL!}p1+h#W*sdc~Dp37M<?6N~8WLz"; 

        public static string GenerateCompetitionToken(string parameter)
        {
            string token = $"{parameter}:{DateTime.UtcNow.Ticks}:{SecretKey}";

            string hashedToken = GetSha256Hash(token);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(hashedToken));
        }

        private static string GetSha256Hash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

}

