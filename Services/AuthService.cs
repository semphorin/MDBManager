using OtpNet;
using QRCoder;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


public interface IAuthService
{
    public string secretKey { get; set; }
    public string UpdateOTPKey();
    public string GenerateSecretKey(int keyLength);
    public byte[] GenerateQrCode();
    public bool ValidateToken(string token);
    public string GenerateJwt();
}
public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    public string secretKey { get; set; }
    
    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
        secretKey = UpdateOTPKey();
    }

    // This needs to be completely redone. We need to scale to multiple users as well as generate a new key
    // when the user wants to reset 2FA.
    // For now, this is a single user system.
    public string UpdateOTPKey()
    {
        var envVar = Environment.GetEnvironmentVariable("OTP_MDB_SECRET");
        if (envVar is not null && envVar.Length > 19)
            return envVar;

        return GenerateSecretKey();
    }

    public string GenerateSecretKey(int keyLength = 20)
    {
        // should only be run if a secret key is not already set!
        var key = KeyGeneration.GenerateRandomKey(keyLength); // 20-byte key
        var encodedKey = Base32Encoding.ToString(key); // convert to string
        if (String.IsNullOrEmpty(encodedKey))
            return "";

        Environment.SetEnvironmentVariable("OTP_MDB_SECRET", this.secretKey, EnvironmentVariableTarget.User);
        return encodedKey;
    }

    public byte[] GenerateQrCode()
    {
        UpdateOTPKey();
        string otpUrl = $"otpauth://totp/MDBManager:singleUser?secret={this.secretKey}&issuer=MDBManager";
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(otpUrl, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(20);
    }

    public bool ValidateToken(string token)
    {
        _logger.LogDebug("Validating OTP token");
        var otp = new Totp(Base32Encoding.ToBytes(this.secretKey));
        bool verified = otp.VerifyTotp(token, out _, new VerificationWindow(1, 1));
        if (!verified)
        {
            _logger.LogWarning("OTP token validation failed");
            return false;
        }

        _logger.LogInformation("OTP token validated successfully");
        return true;
    }
    
    public string GenerateJwt()
    {
        _logger.LogInformation("Generating JWT token");
        var temp = Environment.GetEnvironmentVariable("JWT_MDB_SECRET");
        var jwtKey = "";
        if (string.IsNullOrEmpty(temp))
            throw new Exception("JWT secret is not configured.");

        jwtKey = temp;
        // var claims = new[]
        // {
        //     new Claim(ClaimTypes.Name, "singleUser") // As you only have one user
        // };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "MDBManager",
            audience: "MDBManager",
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        // clear OTP secretkey from memory after validation succeeds and JWT is generated
        this.secretKey = "";
        _logger.LogInformation("JWT token generated successfully");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}