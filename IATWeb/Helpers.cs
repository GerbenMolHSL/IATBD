using System.Security.Cryptography;
using System.Text;

namespace IATWeb;

public static class Helpers
{
    public static string GenerateSHA256(string str)
    {
        // Convert the string to bytes
        byte[] bytes = Encoding.UTF8.GetBytes(str);

        // Create an instance of SHA256
        using (SHA256 sha256 = SHA256.Create())
        {
            // Compute the hash
            byte[] hashBytes = sha256.ComputeHash(bytes);

            // Convert the byte array to a hexadecimal string
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2")); // Convert each byte to a two-digit hexadecimal value
            }
            return stringBuilder.ToString();
        }
    }
}