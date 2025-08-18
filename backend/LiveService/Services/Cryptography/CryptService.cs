using System.Security.Cryptography;

namespace Tweetz.MicroServices.LiveService.Services;

public class CryptService(IConfiguration configuration) : ICryptService
{

    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Generate a cookie signature
    /// </summary>
    /// <param name="cookie">cookie value</param>
    /// <returns>hash</returns>
    public async Task<string> GenerateSignature(string value)
    {
        return await Task.Run(() =>
        {
            string? secretKey = _configuration.GetSection("AppSettings:Secret").Value ?? throw new Exception("AppSettings secret is null");

            using HMACSHA512 hmac = new(Encoding.UTF8.GetBytes(secretKey));
            byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(signature);
        });
    }

    /// <summary>
    /// verify cookie signature hash
    /// </summary>
    /// <param name="signature">cookie signature</param>
    /// <param name="cookie value">value of the cookie</param>
    /// <returns>boolean</returns>
    public async Task<bool> VerifyCookie(string value, string signature)
    {
        return await Task.Run(async () =>
        {
            string expectedSignature = await GenerateSignature(value);
            return signature == expectedSignature;
        });
    }
}