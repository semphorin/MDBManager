using Microsoft.AspNetCore.Mvc;


namespace MDBManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("validate-otp")]
        public IActionResult ValidateOtp([FromBody] string token)
        {
            if (_authService.ValidateToken(token))
            {
                // TODO
                // generate and send JWT
                
                return Ok(_authService.GenerateJwt());
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("generate-qr")]
        public IActionResult GenerateQRCode()
        {
            byte[] qrCode = _authService.GenerateQrCode();
            return File(qrCode, "image/png");
        }
    }
}