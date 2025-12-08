using Contracts.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Configurations;
using Shared.DTOs.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Shared.Common.Constants;

namespace Infrastructure.Identity;

public class TokenService(JwtSettings jwtSettings) : ITokenService
{
    public TokenResponse GetToken(TokenRequest request)
    {
        var token = GenerateJwt();
        var result = new TokenResponse(token);
      return result;
    }

    public TokenResponse GenerateToken(UserInfoDto userInfo)
    {
var claims = new List<Claim>
   {
     new Claim(JwtRegisteredClaimNames.Sub, userInfo.Username),
 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
  new Claim(ClaimTypes.Name, userInfo.Username),
            new Claim(ClaimTypes.Email, userInfo.Email)
        };

        foreach (var role in userInfo.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!string.IsNullOrEmpty(userInfo.FirstName))
     claims.Add(new Claim("FirstName", userInfo.FirstName));
 
        if (!string.IsNullOrEmpty(userInfo.LastName))
      claims.Add(new Claim("LastName", userInfo.LastName));

     if (userInfo.Permissions != null && userInfo.Permissions.Any())
        {
            var permissionsJson = JsonSerializer.Serialize(userInfo.Permissions.ToList());
      claims.Add(new Claim(SystemConstants.Claims.Permissions, permissionsJson));
        }

        var token = GenerateEncryptedToken(GetSigningCredential(), claims);
        var expiresIn = jwtSettings.ExpirationInMinutes * 60;

        return new TokenResponse(token, expiresIn);
    }

    private string GenerateJwt() => GenerateEncryptedToken(GetSigningCredential(), null);

    private SigningCredentials GetSigningCredential()
    {
        byte[] secret = Encoding.UTF8.GetBytes(jwtSettings.Key);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }

    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim>? additionalClaims = null)
  {
        var claims = new List<Claim>();
        
        if (additionalClaims != null)
   {
       claims.AddRange(additionalClaims);
        }
  else
        {
         claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
        }

     var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
   expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
          signingCredentials: signingCredentials);
   
        var tokenHandler = new JwtSecurityTokenHandler();
   return tokenHandler.WriteToken(token);
 }
}