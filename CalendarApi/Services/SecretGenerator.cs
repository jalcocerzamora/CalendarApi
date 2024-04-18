using System.Text;

namespace CalendarApi.Services
{
    public class SecretGenerator
    {
        public string GenerateSecret(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+";
            var random = new Random();
            var secret = new StringBuilder(length);
            for(int i = 0; i < length; i++)
            {
                secret.Append(chars[random.Next(chars.Length)]);
            }
            return secret.ToString();
        }
    }
}
