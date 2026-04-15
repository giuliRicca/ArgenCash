using ArgenCash.Application.Interfaces;
using ArgenCash.Infrastructure.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgenCash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IVerificationTokenService _verificationTokenService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IVerificationTokenService verificationTokenService,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _authService = authService;
        _verificationTokenService = verificationTokenService;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    [HttpPost("register/initiate")]
    public async Task<IActionResult> RegisterInitiate([FromBody] RegistrationInitiateRequest request)
    {
        try
        {
            var existingUser = await _authService.CheckUserExistsAsync(request.Email);
            if (existingUser)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "User already exists.",
                    Detail = "An account with this email already exists.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            var verificationToken = _verificationTokenService.GenerateToken(request);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";

            var htmlBody = EmailTemplates.GetVerificationEmail(verificationToken, frontendUrl);
            await _emailSender.SendEmailAsync(request.Email, "Verifica tu correo en ArgenCash", htmlBody);

            return Ok(new RegistrationInitiateResponse
            {
                Message = "Verification email sent. Please check your inbox.",
                ExpiresAtUtc = expiresAtUtc
            });
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
    }

    [HttpPost("register/verify")]
    public async Task<IActionResult> RegisterVerify([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var registrationData = _verificationTokenService.ValidateToken(request.VerificationToken);
            if (registrationData is null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid or expired token.",
                    Detail = "The verification link is invalid or has expired.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var regRequest = new RegisterUserRequest
            {
                FullName = registrationData.FullName,
                Email = registrationData.Email,
                Password = registrationData.Password
            };

            var response = await _authService.RegisterAsync(regRequest);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "User already exists.",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPost("register/resend")]
    public async Task<IActionResult> RegisterResend([FromBody] ResendVerificationRequest request)
    {
        try
        {
            var verificationToken = _verificationTokenService.GenerateToken(new RegistrationInitiateRequest
            {
                FullName = request.Email,
                Email = request.Email,
                Password = string.Empty
            });

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";

            var htmlBody = EmailTemplates.GetVerificationEmail(verificationToken, frontendUrl);
            await _emailSender.SendEmailAsync(request.Email, "Verifica tu correo en ArgenCash", htmlBody);

            return Ok(new RegistrationInitiateResponse
            {
                Message = "Verification email resent.",
                ExpiresAtUtc = expiresAtUtc
            });
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return ValidationProblem(detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "User already exists.",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed.",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }
}
