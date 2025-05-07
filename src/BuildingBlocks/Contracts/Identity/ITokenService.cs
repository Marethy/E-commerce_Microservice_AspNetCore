using Shared.DTOs.Identity;

namespace Contracts.Identity;

public interface ITokenService
{
    TokenResponse GetToken(TokenRequest request);
}