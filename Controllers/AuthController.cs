using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mini_mes_be.Constants;
using mini_mes_be.DTOs;
using mini_mes_be.DTOs.Auth;
using mini_mes_be.Services;

namespace mini_mes_be.Controllers;

[ApiController]
[Route("v1")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;
    private string IpAddress =>
        Request.Headers.TryGetValue("X-Forwarded-For", out var ip)
            ? ip.ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request, IpAddress);
        return CreatedAtAction(nameof(Register), ApiResponse<TokenResponse>.Ok(result, ErrorMessages.Auth.RegistrationSuccess));
    }

    // ── POST /v1/login ─────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request, IpAddress);
        return Ok(ApiResponse<TokenResponse>.Ok(result, ErrorMessages.Auth.LoginSuccess));
    }

    // ── POST /v1/refresh ───────────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _auth.RefreshTokenAsync(request.refresh_token, IpAddress);
        return Ok(ApiResponse<TokenResponse>.Ok(result, ErrorMessages.Auth.TokenRefreshed));
    }

    // ── POST /v1/revoke ────────────────────────────────────────────────

    [Authorize]
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
    {
        await _auth.RevokeTokenAsync(request.refresh_token, IpAddress);
        return NoContent();
    }

    // ── POST /v1/reset-password ──────────────────────────────────────────

    [AllowAnonymous]
    [HttpPost("reset_password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _auth.ResetPasswordAsync(request);
        return NoContent();
    }

    // ── GET /v1/me ─────────────────────────────────────────────────────

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserMeResponse>), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

        var me = new UserMeResponse(
            int.TryParse(sub, out var id) ? id : null,
            User.Identity?.Name,
            User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        );
        return Ok(ApiResponse<UserMeResponse>.Ok(me));
    }
}
