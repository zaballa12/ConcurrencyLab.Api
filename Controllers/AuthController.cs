using ConcurrencyLab.Api.Data;
using ConcurrencyLab.Api.DTOs;
using ConcurrencyLab.Api.Models;
using ConcurrencyLab.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyLab.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthController(
        AppDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        TokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "username_and_password_required" });
        }

        var user = await _context.Users
            .SingleOrDefaultAsync(currentUser => currentUser.Username == request.Username);

        if (user is null)
        {
            return Unauthorized(new { error = "invalid_credentials" });
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { error = "invalid_credentials" });
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new
        {
            accessToken = token,
            tokenType = "Bearer",
            username = user.Username,
            role = user.Role
        });
    }
}
