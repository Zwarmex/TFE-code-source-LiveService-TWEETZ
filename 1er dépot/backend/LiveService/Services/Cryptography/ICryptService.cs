namespace Tweetz.MicroServices.LiveService.Services
{
    public interface ICryptService
    {
        Task<string> GenerateSignature(string value);
        Task<bool> VerifyCookie(string value, string signature);
    }
}