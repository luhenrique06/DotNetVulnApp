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
            var key = Encoding.ASCII.GetBytes("fedaf7d8863b48e197b9287d492b708e");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserId", user.Id),
                    new Claim(ClaimTypes.Role,  user.IsAdmin ? "admin":"user"),
                    new Claim(ClaimTypes.Name, user.Login)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
               
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
     }
}