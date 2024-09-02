using System;
using System.Security.Cryptography;
using System.Web;

namespace DesktopStreaming.Authentication;

public class SimpleAuthenticationService : IAuthenticationService
{
    private string _currentAuthKey;

    public string GenerateAuthKey()
    {
        using var rng = new RNGCryptoServiceProvider();
        var bytes = new byte[32]; // 256 bits
        rng.GetBytes(bytes);
        _currentAuthKey = Convert.ToBase64String(bytes);
        return HttpUtility.UrlEncode(_currentAuthKey);
    }

    public bool ValidateAuthKey(string authKey)
    {
        string decodedKey = HttpUtility.UrlDecode(authKey);
        return !string.IsNullOrEmpty(decodedKey) && decodedKey == _currentAuthKey;
    }
}