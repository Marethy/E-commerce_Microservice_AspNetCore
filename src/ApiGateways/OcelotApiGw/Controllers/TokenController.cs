using Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Identity;

namespace OcelotApiGw.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TokenController(ITokenService tokenService) : ControllerBase
{

   

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetToken()
    {
        var result = tokenService.GetToken(new TokenRequest());
        return Ok(result);
    }
}