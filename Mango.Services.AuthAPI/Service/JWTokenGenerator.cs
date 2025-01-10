using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mango.Services.AuthAPI.Service
{
    public class JWTokenGenerator : IJWTokenGenerator
    {
        private readonly JwtOptions _JwtOptions;
        public JWTokenGenerator(IOptions<JwtOptions> jwtOptions)
        {
            _JwtOptions = jwtOptions.Value;
        }

        public string GenerateToken(ApplicationUser applicationUsers, IEnumerable<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_JwtOptions.Secret);
            var claim = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Email,applicationUsers.Email),
                new Claim(JwtRegisteredClaimNames.Sub,applicationUsers.Id),
                new Claim(JwtRegisteredClaimNames.Name,applicationUsers.UserName),
            };
            var role = roles.Select(role => new Claim(ClaimTypes.Role, role));
            claim.AddRange(role);

            var tokenDescriptor = new SecurityTokenDescriptor
            {

                Audience = _JwtOptions.Audience,
                Issuer = _JwtOptions.Issuer,
                Subject = new ClaimsIdentity(claim),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
    }
}
