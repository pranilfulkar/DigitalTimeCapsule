using CapsuleAPI.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CapsuleAPI.Helper
{
    public static class JwtTokenHelper
    {
        
        public static string GenerateToken(AppUser user, IConfiguration config)
        {
            //var claims = new[]
            //{
            //    new Claim(ClaimTypes.NameIdentifier, user.Id),
            //    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            //    new Claim(JwtRegisteredClaimNames.Email, user.Email)
            //};

            var claims = new[]
               {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
              //  new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
               };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(40),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
