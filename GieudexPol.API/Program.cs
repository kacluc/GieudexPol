using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Application.Auth.Services;
using GieudexPol.API.Services;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Data;
using GieudexPol.Infrastructure.ExternalServices.Nbp;
using GieudexPol.Infrastructure.Repositories;
using GieudexPol.Infrastructure.Services;
using GieudexPol.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext for production
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Add authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register services
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(AuthService).Assembly));

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<GieudexPol.Domain.Auth.IUserRepository, GieudexPol.Infrastructure.Auth.UserRepository>();

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, GieudexPol.Application.Services.TransactionService>();
builder.Services.AddScoped<IUserAlertService, UserAlertService>();
builder.Services.AddScoped<IExchangeRateSyncService, ExchangeRateSyncService>();

builder.Services.AddHttpClient<IExternalExchangeRateClient, NbpExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["NbpApi:BaseUrl"] ?? "https://api.nbp.pl/api/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    if (!baseUrl.EndsWith("api/", StringComparison.OrdinalIgnoreCase))
    {
        baseUrl += "api/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHostedService<NbpExchangeRateStartupSyncService>();

// Add repositories
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddScoped<IUserRepository, GieudexPol.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUserAlertRepository, UserAlertRepository>();
builder.Services.AddScoped<IRateSourceRepository, RateSourceRepository>();
builder.Services.AddScoped<ITransactionFeeRepository, TransactionFeeRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var hasWebRoot = Directory.Exists(app.Environment.WebRootPath);
var hasSpaIndex = hasWebRoot && File.Exists(Path.Combine(app.Environment.WebRootPath, "index.html"));

if (hasWebRoot)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (hasSpaIndex)
{
    app.MapFallbackToFile("index.html");
}

app.Run();

public partial class Program { }
