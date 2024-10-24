using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using prometeyapi.Core.Services.Background;
using prometeyapi.Core.Services.FirebaseStorageService;
using prometeyapi.Infrastructure.Data;
using prometeyapi.Infrastructure.Services.ApplicationServices;
using prometeyapi.Infrastructure.Services.AuthServices;
using prometeyapi.Infrastructure.Services.GroupServices;
using prometeyapi.Infrastructure.Services.PostServices;
using prometeyapi.Infrastructure.Services.UserServices;
using Serilog;
using System.Text;

Env.Load();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT__Key"));

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2024 * 1024 * 1024;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2024 * 1024 * 1024;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<DBContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<SignUpService>();
builder.Services.AddScoped<LogInService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostsService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<FirebaseStorageService>();
builder.Services.AddHostedService<VerificationTimeoutService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Bearer", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.WithOrigins(
                "http://localhost",
                "https://theprometey.xyz",
                "https://www.theprometey.xyz"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run("http://*:5205");