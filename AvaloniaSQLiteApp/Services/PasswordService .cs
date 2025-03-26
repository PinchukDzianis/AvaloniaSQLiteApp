using AvaloniaSQLiteApp.Services;
using System;
using System.Linq;
using System.Security.Cryptography;

public class PasswordService : IPasswordService
{
    public bool VerifyPassword(string enteredPassword, string storedHash, string saltBase64)
    {
        try
        {
            var salt = Convert.FromBase64String(saltBase64);
            var storedPasswordHash = Convert.FromBase64String(storedHash);

            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000, HashAlgorithmName.SHA256))
            {
                var computedHash = pbkdf2.GetBytes(32);
                return computedHash.SequenceEqual(storedPasswordHash);
            }
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public (string Salt, string Hash) HashPassword(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(32);

            var saltBase64 = Convert.ToBase64String(salt);
            var hashBase64 = Convert.ToBase64String(hash);

            return (saltBase64, hashBase64);
        }
    }
}
