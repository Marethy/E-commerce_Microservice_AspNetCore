using Contracts.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Configurations;
using Shared.DTOs.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public TokenResponse GetToken(TokenRequest request)
    {
        var token = GenerateJwt();
        var result = new TokenResponse(token);
        return result;
    }

    private string GenerateJwt() => GenerateEncryptedToken(GetSigningCredential());

    private SigningCredentials GetSigningCredential()
    {
        byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }

    private string GenerateEncryptedToken(SigningCredentials signingCredentials)
    {
        //var claims = new[]
        //{
        //    new Claim("Role", "Admin")
        //};
        var token = new JwtSecurityToken(
            //claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: signingCredentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}