using System.Security.Claims;
using LoginService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Identity.Client.NativeInterop;

namespace MyApp.Namespace;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    public readonly IAuthService _authService;

    public AuthenticationController(IAuthService authService)
    {
        _authService = authService;
    }

    [EnableRateLimiting("RegisterPolicy")]
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await _authService.CreateUser(request);
        return Created("User registered successfully!",user);
    }

    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var tokenResponse = await _authService.Login(request);
        return Ok(tokenResponse);
    }

    [Authorize]
    [HttpGet("CurrentUser")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(new
        {
            userId,
            userName,
            email
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("AllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var allUsers = await _authService.GetAllUsers();

        return Ok(allUsers);
    }

    [EnableRateLimiting("RefreshPolicy")]
    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest refreshTokenRequest)
    {
        var tokenResponse = await _authService.GetRefreshToken(refreshTokenRequest.RefreshToken);
        return Ok(tokenResponse);
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest refreshTokenRequest)
    {
        await _authService.LogoutAsync(refreshTokenRequest.RefreshToken);

        return NoContent();
    }
}

