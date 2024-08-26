namespace DesktopStreaming.Authentication
{
    public interface IAuthenticationService
    {
        string GenerateAuthKey();
        bool ValidateAuthKey(string authKey);
    }
}