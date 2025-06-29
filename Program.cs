using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using MDBManager.Data;
using MDBManager.Services;
using MDBManager.Utils;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_MDB_SECRET");
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT secret is not configured.");
}
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true
        };
    });
// // Load certificate
// var certPath = Environment.GetEnvironmentVariable("MDB_SSL_CERT");
// var certPassword = Environment.GetEnvironmentVariable("MDB_SSL_PASS");
// var certificate = new X509Certificate2(certPath, certPassword);

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(5091, listenOptions =>
//     {
//         listenOptions.UseHttps(certificate);
//     });
// });

// Configure Database
string dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
Directory.CreateDirectory(dataDirectory);
string connectionString = $"Data Source={Path.Combine(dataDirectory, "mdb.db")}";
builder.Services.AddDbContext<MDBContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IMetadataService, MetadataService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHostedService<MDBManager.Services.TimedBackgroundService>();
builder.WebHost.UseUrls("http://0.0.0.0:5091");

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MDBContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Migrate existing JSON data to database
    await DatabaseMigrationHelper.MigrateJsonToDatabase(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// we love authentication.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
