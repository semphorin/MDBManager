using Microsoft.AspNetCore.Mvc;


namespace MDBManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }        [HttpPost("validate-otp")]
        public IActionResult ValidateOtp([FromBody] string token)
        {
            _logger.LogInformation("OTP validation request received");
            if (!_authService.ValidateToken(token))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid OTP token");
                return Unauthorized();
            }
            
            // generate and send JWT
            _logger.LogInformation("OTP validated successfully, generating JWT token");
            return Ok(_authService.GenerateJwt());
        }

        [HttpGet("generate-qr")]
        public IActionResult GenerateQRCode()
        {
            _logger.LogInformation("QR code generation request received");
            byte[] qrCode = _authService.GenerateQrCode();
            return File(qrCode, "image/png");
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint accessed");
            return Ok("Test successful");
        }
    }
}