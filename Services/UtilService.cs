using Dapper;
using brokenaccesscontrol.Models;
using System.Security.Cryptography;
using System.Text;

namespace brokenaccesscontrol.Services;

public static class UtilService
{
    public static string ReturnSha512(string value){        
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var sha512 = SHA512.Create();
        var hash = sha512.ComputeHash(valueBytes);
        return Convert.ToBase64String(hash);
    }
}
