using System.Security.Cryptography;
using System.Text;

namespace brokenaccesscontrol.Services;

public static class UtilService
{
    // A04 - Cryptographic Failures: MD5 sem sal, saída Base64. Rainbow-table trivial.
    public static string ReturnMD5(string value){
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var hashmd = MD5.Create();
        var hash = hashmd.ComputeHash(valueBytes);
        return Convert.ToBase64String(hash);
    }

    // A04 - Token de recuperação PREVISÍVEL: MD5(login + data do dia).
    // Atacante que conhece o login reproduz o token offline.
    public static string GenerateResetToken(string login){
        var seed = login + DateTime.UtcNow.ToString("yyyyMMdd");
        var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexString(bytes).ToLower();
    }
}
