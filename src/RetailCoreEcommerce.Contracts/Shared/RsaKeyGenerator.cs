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
        var rsa = RSA.Create();
        try
        {
            // Try to decode Base64 first
            var keyBytes = Convert.FromBase64String(encodedKey);
            var keyPem = Encoding.UTF8.GetString(keyBytes);
            rsa.ImportFromPem(keyPem);
        }
        catch (FormatException)
        {
            throw new Exception("Invalid key format.");
        }

        return rsa;
    }
}