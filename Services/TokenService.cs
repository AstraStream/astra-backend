using System.Text;
using Astra.Models;
using System.Security.Claims;
using Astra.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Astra.Services {
    public class TokenService: ITokenService {

        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessExpiry;
        private readonly int _refreshExpiry;
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config) {
            _config = config;
            _issuer = _config["Jwt:Issuer"]!;
            _audience = _config["Jwt:Audience"]!;
            _accessExpiry = _config.GetValue<int>("Jwt:ExpiryAccess");
            _refreshExpiry = _config.GetValue<int>("Jwt:ExpiryRefresh");
            _key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]!)
            );
        }

         public string CreateAccessToken(User user, IList<string> roles) {
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // add user roles to claim
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));


            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_accessExpiry),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature),
                Issuer = _issuer,
                Audience = _audience,
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public string CreateRefreshToken(string email)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Email, email!)
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_refreshExpiry),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature),
                Issuer = _issuer,
                Audience = _audience,
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}