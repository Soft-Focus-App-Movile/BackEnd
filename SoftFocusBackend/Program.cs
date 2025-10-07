using DotNetEnv;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Configuration;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Configuration;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;
using SoftFocusBackend.Shared.Interfaces.ASP.Configuration;
using SoftFocusBackend.Auth.Application.ACL.Services;
using SoftFocusBackend.Auth.Application.Internal.CommandServices;
using SoftFocusBackend.Auth.Application.Internal.QueryServices;
using SoftFocusBackend.Auth.Application.Internal.OutboundServices;
using SoftFocusBackend.Auth.Domain.Services;
using SoftFocusBackend.Auth.Infrastructure.ACL;
using SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Configuration;
using SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Services;
using SoftFocusBackend.Auth.Infrastructure.OAuth.Configuration;
using SoftFocusBackend.Auth.Infrastructure.OAuth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using SoftFocusBackend.Users.Application.ACL.Services;
using SoftFocusBackend.Users.Application.Internal.CommandServices;
using SoftFocusBackend.Users.Application.Internal.OutboundServices;
using SoftFocusBackend.Users.Application.Internal.QueryServices;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.ACL;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Users.Infrastructure.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",      
                "http://localhost:8080",      
                "http://localhost:3000",
                "https://softfocus.netlify.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuthSettings"));
builder.Services.Configure<FacebookOAuthSettings>(builder.Configuration.GetSection("FacebookOAuthSettings"));
builder.Services.AddHttpClient<GoogleOAuthService>();
builder.Services.AddHttpClient<FacebookOAuthService>();
builder.Services.AddScoped<GoogleOAuthService>();
builder.Services.AddScoped<FacebookOAuthService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Configuration["GoogleOAuthSettings:ClientId"] = Environment.GetEnvironmentVariable("GoogleOAuthSettings__ClientId");
builder.Configuration["GoogleOAuthSettings:ClientSecret"] = Environment.GetEnvironmentVariable("GoogleOAuthSettings__ClientSecret");
builder.Configuration["FacebookOAuthSettings:AppId"] = Environment.GetEnvironmentVariable("FacebookOAuthSettings__AppId");
builder.Configuration["FacebookOAuthSettings:AppSecret"] = Environment.GetEnvironmentVariable("FacebookOAuthSettings__AppSecret");

// Users - Domain Services
builder.Services.AddScoped<IUserDomainService, UserDomainService>();
builder.Services.AddScoped<IPsychologistVerificationService, PsychologistVerificationService>();
builder.Services.AddScoped<IInvitationCodeService, InvitationCodeService>();

// Users - Application Services  
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IPsychologistCommandService, PsychologistCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IPsychologistQueryService, PsychologistQueryService>();
builder.Services.AddScoped<IUserFacade, UserFacade>();

// Users - Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPsychologistRepository, PsychologistRepository>();

// Users - ACL
builder.Services.AddScoped<IAuthNotificationService, AuthNotificationService>();

builder.Services.AddScoped<IUserFacade, UserFacade>();

var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>();
if (tokenSettings == null || !tokenSettings.IsValid())
{
    throw new InvalidOperationException("JWT TokenSettings are missing or invalid");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = tokenSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),
            ValidateIssuer = tokenSettings.ValidateIssuer,
            ValidIssuer = tokenSettings.Issuer,
            ValidateAudience = tokenSettings.ValidateAudience,
            ValidAudience = tokenSettings.Audience,
            ValidateLifetime = tokenSettings.ValidateLifetime,
            ClockSkew = TimeSpan.FromMinutes(tokenSettings.ClockSkewMinutes)
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SoftFocus API",
        Version = "v1",
        Description = "Soft Focus - Tu acompañamiento emocional API",
        Contact = new OpenApiContact
        {
            Name = "PsyWell Team",
            Email = "contact@softfocus.com"
        }
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });

    options.EnableAnnotations();

    // Configure Swagger to use string names for enums instead of integers
    options.UseInlineDefinitionsForEnums();
});

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IGenericEmailService, GenericEmailService>();
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthCommandService, AuthCommandService>();
builder.Services.AddScoped<IAuthQueryService, AuthQueryService>();
builder.Services.AddScoped<IAuthFacade, AuthFacade>();

builder.Services.AddHttpClient<GoogleOAuthService>();
builder.Services.AddHttpClient<FacebookOAuthService>();
builder.Services.AddScoped<IOAuthService, GoogleOAuthService>(provider =>
    provider.GetRequiredService<GoogleOAuthService>());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();