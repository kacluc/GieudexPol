using GieudexPol.Application.Interfaces;
using GieudexPol.Application.Services;
using GieudexPol.Application.Settings;
using GieudexPol.Application.Auth.Services;
using GieudexPol.API.Middleware;
using GieudexPol.API.Services;
using GieudexPol.Infrastructure;
using GieudexPol.Infrastructure.Data;
using GieudexPol.Infrastructure.ExternalServices.BankOfCanada;
using GieudexPol.Infrastructure.ExternalServices.BankOfEngland;
using GieudexPol.Infrastructure.ExternalServices.Bnr;
using GieudexPol.Infrastructure.ExternalServices.Cnb;
using GieudexPol.Infrastructure.ExternalServices.Ecb;
using GieudexPol.Infrastructure.ExternalServices.Nbp;
using GieudexPol.Infrastructure.ExternalServices.Norges;
using GieudexPol.Infrastructure.ExternalServices.Riksbank;
using GieudexPol.Infrastructure.Repositories;
using GieudexPol.Infrastructure.Services;
using GieudexPol.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<ExchangeRateSettings>(
    builder.Configuration.GetSection(ExchangeRateSettings.SectionName));

// Add DbContext for production
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Add authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
var key = Encoding.ASCII.GetBytes(jwtSecret);
//builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

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
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminTestExchangeRateService, AdminTestExchangeRateService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, GieudexPol.Application.Services.TransactionService>();
builder.Services.AddScoped<ITransactionFeeCalculator, TransactionFeeCalculator>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserAlertService, UserAlertService>();
builder.Services.AddScoped<IExchangeRateSyncService, ExchangeRateSyncService>();
builder.Services.AddScoped<IWhaleRankingService, WhaleRankingService>();

builder.Services.AddHttpClient<NbpExchangeRateClient>(client =>
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

builder.Services.AddHttpClient<EcbExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["EcbApi:BaseUrl"] ?? "https://www.ecb.europa.eu/stats/eurofxref/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
});

builder.Services.AddHttpClient<RiksbankExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["RiksbankApi:BaseUrl"] ?? "https://api.riksbank.se/swea/v1/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var apiKey = builder.Configuration["RiksbankApi:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
    }
});

builder.Services.AddHttpClient<BankOfEnglandExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["BankOfEnglandApi:BaseUrl"] ?? "https://www.bankofengland.co.uk/boeapps/database/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/csv"));
});

builder.Services.AddHttpClient<BankOfCanadaExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["BankOfCanadaApi:BaseUrl"] ?? "https://www.bankofcanada.ca/valet/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<CnbExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["CnbApi:BaseUrl"] ?? "https://api.cnb.cz/cnbapi/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<NorgesExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["NorgesApi:BaseUrl"] ?? "https://data.norges-bank.no/api/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<BnrExchangeRateClient>(client =>
{
    var baseUrl = builder.Configuration["BnrApi:BaseUrl"] ?? "https://curs.bnr.ro/";
    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
});

builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<NbpExchangeRateClient>());
builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<EcbExchangeRateClient>());
builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<RiksbankExchangeRateClient>());
builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<BankOfEnglandExchangeRateClient>());
builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<BankOfCanadaExchangeRateClient>());
builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<CnbExchangeRateClient>());
builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<NorgesExchangeRateClient>());
builder.Services.AddTransient<IExternalExchangeRateClient>(serviceProvider =>
    serviceProvider.GetRequiredService<BnrExchangeRateClient>());

builder.Services.AddHostedService<ExchangeRateStartupSyncService>();

// Add repositories
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddScoped<IUserRepository, GieudexPol.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUserAlertRepository, UserAlertRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IWhaleRankingRepository, WhaleRankingRepository>();
builder.Services.AddScoped<IRateSourceRepository, RateSourceRepository>();
builder.Services.AddScoped<ITransactionFeeRepository, TransactionFeeRepository>();
builder.Services.AddScoped<ICurrencyExchangeSimulationService, CurrencyExchangeSimulationService>();
builder.Services.AddScoped<IFavoriteCurrencyRepository, FavoriteCurrencyRepository>();
builder.Services.AddScoped<FavoriteCurrencyService>();

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

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (hasSpaIndex)
{
    app.MapFallbackToFile("index.html");
}

app.Run();

public partial class Program { }
