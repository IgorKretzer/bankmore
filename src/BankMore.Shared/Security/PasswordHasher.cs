using System.Security.Cryptography;
using System.Text;

namespace BankMore.Shared.Security;

public class PasswordHasher
{
    public static (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        var salt = Convert.ToBase64String(saltBytes);

        var passwordWithSalt = password + salt;
        var passwordBytes = Encoding.UTF8.GetBytes(passwordWithSalt);
        
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(passwordBytes);
            var hash = Convert.ToBase64String(hashBytes);
            return (hash, salt);
        }
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        var passwordWithSalt = password + salt;
        var passwordBytes = Encoding.UTF8.GetBytes(passwordWithSalt);
        
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(passwordBytes);
            var computedHash = Convert.ToBase64String(hashBytes);
            return computedHash == hash;
        }
    }
}
