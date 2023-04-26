using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using brokenaccesscontrol.Models;
using Microsoft.IdentityModel.Tokens;

namespace brokenaccesscontrol.Services
{
     public static class TokenService
     {
        public static string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWTSecret"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserId", user.Id),
                    new Claim(ClaimTypes.Role,  user.IsAdmin ? "admin":"user")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
               // SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
     }
}