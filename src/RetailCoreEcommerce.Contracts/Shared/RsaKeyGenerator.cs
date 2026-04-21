using System.Security.Cryptography;
using System.Text;

namespace RetailCoreEcommerce.Contracts.Shared;

public static class RsaKeyGenerator
{
    /// <summary>
    ///     Read RSA key from Base64 string
    /// </summary>
    /// <param name="encodedKey"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static RSA ReadRsaKeyBase64(this string encodedKey)
    {
        if (string.IsNullOrWhiteSpace(encodedKey))
            throw new ArgumentNullException(nameof(encodedKey), "The encoded key cannot be null or empty.");

        // 1. Remove accidental spaces or newlines from config copy/pasting
        encodedKey = encodedKey.Trim();

        var rsa = RSA.Create();

        try
        {
            // 2. Decode and Import
            var keyBytes = Convert.FromBase64String(encodedKey);
            var keyPem = Encoding.UTF8.GetString(keyBytes);
            rsa.ImportFromPem(keyPem);

            // 3. Only return if everything succeeded
            return rsa;
        }
        catch (Exception ex) // Catches FormatException, CryptographicException, etc.
        {
            // 4. CRITICAL: Dispose the RSA object if it failed to prevent memory leaks
            rsa.Dispose();

            // 5. Pass the 'ex' as the inner exception so you can see exactly what went wrong
            throw new InvalidOperationException(
                "Failed to read the RSA key from the provided Base64 string. See inner exception for details.", ex);
        }
    }
}