using Microsoft.IdentityModel.Tokens;
using SecureBlog.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SecureBlog.API.Services;

public class TokenService
{
    // TODO: JWT üretimi burada implement edilecek
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user)
    {
        //1. claim'ler:
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email,user.Email),
            new Claim(ClaimTypes.Role,user.Role)
        };

        //2. SigninCredentials

        string secretKey = _config["Jwt:Secret"];

   


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //3. token:

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddMinutes(20),
            signingCredentials: credentials

            );




        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);

    }
}
