using Jam.Api.Models;
using Jam.Api.DTOs.Auth;
using Jam.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AuthUser> _userManager;
    private readonly SignInManager<AuthUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<AuthUser> userManager,
        SignInManager<AuthUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthController> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    // ---------------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var user = new AuthUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("[AuthController] user registered for {@username}", registerDto.Username);
            return Ok(new { Message = "User registered successfully" });
        }

        _logger.LogWarning("[AuthAPIController] registration failed for {@username}", registerDto.Username);
        return BadRequest(result.Errors);
    }



    // ---------------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username);

        if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            _logger.LogInformation("[AuthAPIController] user authorised for {@username}", loginDto.Username);
            var token = await _tokenService.GenerateJwtTokenAsync(user);
            return Ok(new { Token = token });
        }

        _logger.LogWarning("[AuthAPIController] user not authorised for {@username}", loginDto.Username);
        return Unauthorized();
    }



    // ---------------------------------------------------------------
    // LOGOUT
    // ---------------------------------------------------------------
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("[AuthAPIController] user logged out");
        return Ok(new { Message = "Logout successful" });
    }
}
