using DotNetEnv;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence;
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
using SoftFocusBackend.AI.Application.ACL.Implementations;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Application.Internal.CommandServices;
using SoftFocusBackend.AI.Application.Internal.QueryServices;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.CrisisDetection;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.Gemini.Configuration;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.Gemini.Services;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.HuggingFace.Configuration;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.HuggingFace.Services;
using SoftFocusBackend.AI.Infrastructure.ExternalServices.UsageTracking;
using SoftFocusBackend.AI.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Tracking.Application.ACL.Services;
using SoftFocusBackend.Tracking.Application.Internal.CommandServices;
using SoftFocusBackend.Tracking.Application.Internal.OutboundServices;
using SoftFocusBackend.Tracking.Application.Internal.QueryServices;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Infrastructure.ACL;
using SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Tracking.Infrastructure.Services;
using SoftFocusBackend.Library.Application.ACL.Implementations;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Application.Internal.CommandServices;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Library.Domain.Services;
using SoftFocusBackend.Library.Infrastructure.Configuration;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Configuration;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Configuration;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Configuration;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.OpenWeather.Configuration;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.OpenWeather.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Foursquare.Configuration;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Foursquare.Services;
using SoftFocusBackend.Library.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Library.Infrastructure.Services;
using SoftFocusBackend.Notification.Application.ACL.Services;
using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Application.Internal.QueryServices;
using SoftFocusBackend.Notification.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Services;
using SoftFocusBackend.Notification.Infrastructure.ACL;
using SoftFocusBackend.Notification.Infrastructure.BackgroundServices;
using SoftFocusBackend.Notification.Infrastructure.ExternalServices;
using SoftFocusBackend.Notification.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Notification.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using SoftFocusBackend.Therapy.Interfaces.REST.Hubs;
using MongoDB.Bson.Serialization;
using SoftFocusBackend.Users.Domain.Model.Aggregates;


Env.Load();

if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
{
    BsonClassMap.RegisterClassMap<User>(cm =>
    {
        cm.AutoMap();
        cm.SetIsRootClass(true);
        cm.AddKnownType(typeof(PsychologistUser));
    });
}

if (!BsonClassMap.IsClassMapRegistered(typeof(PsychologistUser)))
{
    BsonClassMap.RegisterClassMap<PsychologistUser>(cm =>
    {
        cm.AutoMap();
    });
}

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
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.Configure<HuggingFaceSettings>(builder.Configuration.GetSection("HuggingFaceSettings"));
builder.Services.AddHttpClient<GoogleOAuthService>();
builder.Services.AddHttpClient<FacebookOAuthService>();
builder.Services.AddScoped<GoogleOAuthService>();
builder.Services.AddScoped<FacebookOAuthService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Configuration["GoogleOAuthSettings:ClientId"] = Environment.GetEnvironmentVariable("GoogleOAuthSettings__ClientId");
builder.Configuration["GoogleOAuthSettings:ClientSecret"] = Environment.GetEnvironmentVariable("GoogleOAuthSettings__ClientSecret");
builder.Configuration["FacebookOAuthSettings:AppId"] = Environment.GetEnvironmentVariable("FacebookOAuthSettings__AppId");
builder.Configuration["FacebookOAuthSettings:AppSecret"] = Environment.GetEnvironmentVariable("FacebookOAuthSettings__AppSecret");
builder.Configuration["GeminiSettings:ApiKey"] = Environment.GetEnvironmentVariable("GeminiSettings__ApiKey");
builder.Configuration["HuggingFaceSettings:ApiToken"] = Environment.GetEnvironmentVariable("HuggingFaceSettings__ApiToken");
builder.Configuration["TMDB:ApiKey"]= Environment.GetEnvironmentVariable("TMDB__ApiKey");
builder.Configuration["Spotify:ClientId"] = Environment.GetEnvironmentVariable("Spotify__ClientId");
builder.Configuration["Spotify:ClientSecret"] = Environment.GetEnvironmentVariable("Spotify__ClientSecret");
builder.Configuration["YouTube:ApiKey"] = Environment.GetEnvironmentVariable("YouTube__ApiKey");
builder.Configuration["OpenWeather:ApiKey"] = Environment.GetEnvironmentVariable("OpenWeather__ApiKey");
builder.Configuration["Foursquare:ApiKey"] = Environment.GetEnvironmentVariable("Foursquare__ApiKey");
builder.Configuration["StripeSettings:PublishableKey"] = Environment.GetEnvironmentVariable("StripeSettings__PublishableKey");
builder.Configuration["StripeSettings:SecretKey"] = Environment.GetEnvironmentVariable("StripeSettings__SecretKey");
builder.Configuration["StripeSettings:WebhookSecret"] = Environment.GetEnvironmentVariable("StripeSettings__WebhookSecret");
builder.Configuration["StripeSettings:ProPriceId"] = Environment.GetEnvironmentVariable("StripeSettings__ProPriceId");

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

// Useers -   ACL

builder.Services.AddScoped<IAuthNotificationService, AuthNotificationService>();

// Tracking - Domain Services
builder.Services.AddScoped<ITrackingDomainService, TrackingDomainService>();

// Tracking - Application Services
builder.Services.AddScoped<ICheckInCommandService, CheckInCommandService>();
builder.Services.AddScoped<ICheckInQueryService, CheckInQueryService>();
builder.Services.AddScoped<IEmotionalCalendarCommandService, EmotionalCalendarCommandService>();
builder.Services.AddScoped<IEmotionalCalendarQueryService, EmotionalCalendarQueryService>();

// Tracking - Repositories
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<IEmotionalCalendarRepository, EmotionalCalendarRepository>();

// Tracking - ACL
builder.Services.AddScoped<IUserValidationService, UserValidationService>();
builder.Services.AddScoped<ITrackingNotificationService, TrackingNotificationService>();

// Tracking - Facade
builder.Services.AddScoped<ITrackingFacade, TrackingFacade>();



builder.Services.AddScoped<IUserFacade, UserFacade>();

// AI - Configuration & HttpClients
builder.Services.AddHttpClient<GeminiChatService>();
builder.Services.AddHttpClient<HuggingFaceEmotionService>();

// AI - Domain Services
builder.Services.AddScoped<IEmotionalChatService, GeminiChatService>();
builder.Services.AddScoped<IFacialEmotionService, HuggingFaceEmotionService>();
builder.Services.AddScoped<IAIUsageTracker, AIUsageTrackerService>();
builder.Services.AddScoped<IGeminiContextBuilder, GeminiContextBuilder>();
builder.Services.AddScoped<ICrisisPatternDetector, CrisisPatternDetectorService>();

// AI - Repositories
builder.Services.AddScoped<IChatSessionRepository, MongoChatSessionRepository>();
builder.Services.AddScoped<IEmotionAnalysisRepository, MongoEmotionAnalysisRepository>();
builder.Services.AddScoped<IAIUsageRepository, MongoAIUsageRepository>();

// AI - Application Services
builder.Services.AddScoped<AIChatCommandService>();
builder.Services.AddScoped<AIEmotionCommandService>();
builder.Services.AddScoped<AIUsageQueryService>();
builder.Services.AddScoped<AIChatQueryService>();

// AI - ACL Services (Mock implementations)
builder.Services.AddScoped<SoftFocusBackend.AI.Application.ACL.Services.ITrackingIntegrationService, SoftFocusBackend.AI.Application.ACL.Implementations.TrackingIntegrationService>();
builder.Services.AddScoped<ITherapyIntegrationService, TherapyIntegrationService>();
builder.Services.AddScoped<ICrisisIntegrationService, CrisisIntegrationService>();
builder.Services.AddScoped<ISubscriptionIntegrationService, SubscriptionIntegrationService>();

// ============================================
// CRISIS BOUNDED CONTEXT
// ============================================

builder.Services.AddScoped<SoftFocusBackend.Crisis.Domain.Repositories.ICrisisAlertRepository, SoftFocusBackend.Crisis.Infrastructure.Persistence.CrisisAlertRepository>();

builder.Services.AddScoped<SoftFocusBackend.Crisis.Domain.Services.ICrisisNotificationService, SoftFocusBackend.Crisis.Infrastructure.Services.CrisisNotificationService>();

builder.Services.AddScoped<SoftFocusBackend.Crisis.Application.Internal.CommandServices.ICrisisAlertCommandService, SoftFocusBackend.Crisis.Application.Internal.CommandServices.CrisisAlertCommandService>();
builder.Services.AddScoped<SoftFocusBackend.Crisis.Application.Internal.QueryServices.ICrisisAlertQueryService, SoftFocusBackend.Crisis.Application.Internal.QueryServices.CrisisAlertQueryService>();

builder.Services.AddScoped<SoftFocusBackend.Crisis.Application.ACL.ICrisisIntegrationService, SoftFocusBackend.Crisis.Application.ACL.CrisisIntegrationService>();

builder.Services.AddScoped<SoftFocusBackend.Crisis.Interfaces.REST.Transform.CrisisAlertResourceFromEntityAssembler>();

// ============================================
// LIBRARY BOUNDED CONTEXT
// ============================================

// Library - Configuration Settings
builder.Services.Configure<TMDBSettings>(builder.Configuration.GetSection("TMDB"));
builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.Configure<YouTubeSettings>(builder.Configuration.GetSection("YouTube"));
builder.Services.Configure<OpenWeatherSettings>(builder.Configuration.GetSection("OpenWeather"));
builder.Services.Configure<FoursquareSettings>(builder.Configuration.GetSection("Foursquare"));
builder.Services.Configure<LibraryCacheSettings>(builder.Configuration.GetSection("LibraryCacheSettings"));

// Library - External Services (HttpClient configured)
builder.Services.AddHttpClient<ITMDBService, TMDBMovieService>();
builder.Services.AddHttpClient<ISpotifyService, SpotifyMusicService>();
builder.Services.AddHttpClient<IYouTubeService, YouTubeVideoService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddHttpClient<IFoursquareService, FoursquarePlacesService>();

// Library - Domain Services
builder.Services.AddScoped<IContentSearchService, ContentSearchService>();
builder.Services.AddScoped<IEmotionContentMatcher, EmotionContentMatcherService>();
builder.Services.AddScoped<IWeatherPlaceRecommender, WeatherPlaceRecommenderService>();
builder.Services.AddScoped<IContentCacheService, ContentCacheService>();
builder.Services.AddScoped<ICachePopulationService, CachePopulationService>();

// Library - Repositories
builder.Services.AddScoped<IContentItemRepository, MongoContentItemRepository>();
builder.Services.AddScoped<IUserFavoriteRepository, MongoUserFavoriteRepository>();
builder.Services.AddScoped<IContentAssignmentRepository, MongoContentAssignmentRepository>();
builder.Services.AddScoped<IContentCompletionRepository, MongoContentCompletionRepository>();

// Library - Application Command Services
builder.Services.AddScoped<IFavoriteCommandService, FavoriteCommandService>();
builder.Services.AddScoped<IAssignmentCommandService, AssignmentCommandService>();
builder.Services.AddScoped<ICompletionCommandService, CompletionCommandService>();

// Library - Application Query Services
builder.Services.AddScoped<IContentSearchQueryService, ContentSearchQueryService>();
builder.Services.AddScoped<IFavoriteQueryService, FavoriteQueryService>();
builder.Services.AddScoped<IAssignedContentQueryService, AssignedContentQueryService>();
builder.Services.AddScoped<IRecommendationQueryService, RecommendationQueryService>();

// Library - ACL Services
builder.Services.AddScoped<SoftFocusBackend.Library.Application.ACL.Services.IUserIntegrationService, SoftFocusBackend.Library.Application.ACL.Implementations.UserIntegrationService>();
builder.Services.AddScoped<SoftFocusBackend.Library.Application.ACL.Services.ITrackingIntegrationService, SoftFocusBackend.Library.Application.ACL.Implementations.TrackingIntegrationService>();

// ============================================
// SUBSCRIPTION BOUNDED CONTEXT
// ============================================

// Subscription - Configuration
builder.Services.Configure<SoftFocusBackend.Subscription.Infrastructure.ExternalServices.StripeSettings>(
    builder.Configuration.GetSection("StripeSettings"));

// Subscription - Repositories
builder.Services.AddScoped<SoftFocusBackend.Subscription.Infrastructure.Repositories.ISubscriptionRepository,
    SoftFocusBackend.Subscription.Infrastructure.Repositories.SubscriptionRepository>();
builder.Services.AddScoped<SoftFocusBackend.Subscription.Infrastructure.Repositories.IUsageTrackingRepository,
    SoftFocusBackend.Subscription.Infrastructure.Repositories.UsageTrackingRepository>();

// Subscription - External Services
builder.Services.AddScoped<SoftFocusBackend.Subscription.Infrastructure.ExternalServices.IStripePaymentService,
    SoftFocusBackend.Subscription.Infrastructure.ExternalServices.StripePaymentService>();

// Subscription - Application Services
builder.Services.AddScoped<SoftFocusBackend.Subscription.Application.Services.ISubscriptionCommandService,
    SoftFocusBackend.Subscription.Application.Services.SubscriptionCommandService>();
builder.Services.AddScoped<SoftFocusBackend.Subscription.Application.Services.ISubscriptionQueryService,
    SoftFocusBackend.Subscription.Application.Services.SubscriptionQueryService>();

// ============================================
// THERAPY BOUNDED CONTEXT
// ============================================

// Therapy - Domain Services
builder.Services.AddScoped<SoftFocusBackend.Therapy.Domain.Services.IConnectionValidationService,
    SoftFocusBackend.Therapy.Domain.Services.ConnectionValidationService>();
builder.Services.AddScoped<SoftFocusBackend.Therapy.Domain.Services.IChatModerationService,
    SoftFocusBackend.Therapy.Domain.Services.ChatModerationService>();

// Therapy - Repositories
builder.Services.AddScoped<SoftFocusBackend.Therapy.Domain.Repositories.ITherapeuticRelationshipRepository,
    SoftFocusBackend.Therapy.Infrastructure.Persistence.MongoDB.Repositories.MongoTherapeuticRelationshipRepository>();
builder.Services.AddScoped<SoftFocusBackend.Therapy.Domain.Repositories.IChatMessageRepository,
    SoftFocusBackend.Therapy.Infrastructure.Persistence.MongoDB.Repositories.MongoChatMessageRepository>();

// Therapy - Application Services
builder.Services.AddScoped<SoftFocusBackend.Therapy.Application.Internal.CommandServices.EstablishConnectionCommandService>();
builder.Services.AddScoped<SoftFocusBackend.Therapy.Application.Internal.CommandServices.TerminateRelationshipCommandService>();
builder.Services.AddScoped<SoftFocusBackend.Therapy.Application.Internal.CommandServices.SendChatMessageCommandService>();
builder.Services.AddScoped<SoftFocusBackend.Therapy.Application.Internal.QueryServices.ChatHistoryQueryService>();
builder.Services.AddScoped<SoftFocusBackend.Therapy.Application.Internal.QueryServices.PatientDirectoryQueryService>();
builder.Services.AddScoped<SoftFocusBackend.Therapy.Application.Internal.OutboundServices.IPatientFacade, SoftFocusBackend.Therapy.Infrastructure.ACL.Services.PatientFacade>();

// Add services to the container
builder.Services.AddSignalR(); // Add SignalR services
builder.Services.AddControllers();



// HttpContextAccessor for user context
builder.Services.AddHttpContextAccessor();

// Notification Context Registration
builder.Services.AddScoped<INotificationRepository, MongoNotificationRepository>();
builder.Services.AddScoped<INotificationPreferenceRepository, MongoNotificationPreferenceRepository>();
builder.Services.AddScoped<INotificationTemplateRepository, MongoNotificationTemplateRepository>();

builder.Services.AddScoped<INotificationSchedulingService, NotificationSchedulingService>();
builder.Services.AddScoped<IDeliveryOptimizationService, DeliveryOptimizationService>();

builder.Services.AddScoped<SendNotificationCommandService>();
builder.Services.AddScoped<UpdatePreferencesCommandService>();
builder.Services.AddScoped<NotificationHistoryQueryService>();
builder.Services.AddScoped<PreferenceQueryService>();

builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();

builder.Services.AddSingleton<FirebaseFCMService>(sp => 
    new FirebaseFCMService(
        builder.Configuration["Firebase:ServerKey"],
        sp.GetRequiredService<HttpClient>()
    )
);
builder.Services.AddScoped<EmailNotificationService>();

builder.Services.AddHostedService<NotificationDeliveryService>();

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

    // Map IFormFile to binary in schemas
    options.MapType<Microsoft.AspNetCore.Http.IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    options.MapType<List<Microsoft.AspNetCore.Http.IFormFile>>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "array",
        Items = new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        }
    });

    // Configure Swagger to handle file uploads - must be last
    options.OperationFilter<FileUploadOperationFilter>();
});

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddScoped<IGenericEmailService, GenericEmailService>();
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

// In-memory cache for OAuth temp tokens
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthCommandService, AuthCommandService>();
builder.Services.AddScoped<IAuthQueryService, AuthQueryService>();
builder.Services.AddScoped<IAuthFacade, AuthFacade>();

builder.Services.AddHttpClient<GoogleOAuthService>();
builder.Services.AddHttpClient<FacebookOAuthService>();
builder.Services.AddScoped<IOAuthService, GoogleOAuthService>(provider =>
    provider.GetRequiredService<GoogleOAuthService>());
builder.Services.AddScoped<SoftFocusBackend.Auth.Infrastructure.OAuth.Services.IOAuthTempTokenService, SoftFocusBackend.Auth.Infrastructure.OAuth.Services.OAuthTempTokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var userContextService = scope.ServiceProvider.GetRequiredService<IUserContextService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var adminExists = await userContextService.GetUserByEmailAsync("admin@softfocus.com");
        if (adminExists == null)
        {
            await userContextService.CreateUserAsync(
                email: "admin@softfocus.com",
                password: "Admin123!",
                fullName: "Admin SoftFocus",
                userType: "Admin"
            );
            logger.LogInformation("Admin user created successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not create admin user on startup");
    }

    // Seed database in all environments (development and production)
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
        logger.LogInformation("Database seeding executed successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not seed database on startup");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/chatHub");
app.MapHub<SoftFocusBackend.Crisis.Interfaces.Hubs.CrisisHub>("/crisisHub");

app.Run();

// Parameter filter to skip IFormFile parameters - they'll be handled by OperationFilter
public class FormFileParameterFilter : Swashbuckle.AspNetCore.SwaggerGen.IParameterFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiParameter parameter, Swashbuckle.AspNetCore.SwaggerGen.ParameterFilterContext context)
    {
        var paramType = context.ParameterInfo?.ParameterType;

        // Mark IFormFile parameters to be ignored - the OperationFilter will handle them
        if (paramType == typeof(Microsoft.AspNetCore.Http.IFormFile) ||
            paramType == typeof(List<Microsoft.AspNetCore.Http.IFormFile>))
        {
            // This parameter will be removed by setting it to null
            parameter.Schema = null;
        }
    }
}

// Schema filter to handle IFormFile types
public class FileUploadSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type == typeof(Microsoft.AspNetCore.Http.IFormFile) ||
            context.Type == typeof(List<Microsoft.AspNetCore.Http.IFormFile>))
        {
            schema.Type = "string";
            schema.Format = "binary";
        }
    }
}

// Custom Swagger filter for file uploads
public class FileUploadOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var hasFormFile = context.ApiDescription.ParameterDescriptions.Any(p =>
            p.ModelMetadata?.ModelType == typeof(Microsoft.AspNetCore.Http.IFormFile) ||
            p.ModelMetadata?.ModelType == typeof(List<Microsoft.AspNetCore.Http.IFormFile>));

        if (!hasFormFile)
            return;

        // Clear existing parameters to avoid conflicts
        operation.Parameters?.Clear();

        var properties = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiSchema>();
        var requiredProps = new HashSet<string>();

        foreach (var param in context.ApiDescription.ParameterDescriptions)
        {
            var paramType = param.ModelMetadata?.ModelType;
            var paramName = param.Name;

            if (paramType == typeof(Microsoft.AspNetCore.Http.IFormFile))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = param.ModelMetadata?.Description
                };
                // Check if it's required (not nullable)
                if (!param.ParameterDescriptor.ParameterType.IsGenericType ||
                    param.ParameterDescriptor.ParameterType.GetGenericTypeDefinition() != typeof(Nullable<>))
                {
                    var defaultValue = param.ParameterDescriptor.BindingInfo?.BindingSource;
                    if (param.ParameterDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor controllerParam)
                    {
                        if (!controllerParam.ParameterInfo.IsOptional && !controllerParam.ParameterInfo.HasDefaultValue)
                        {
                            requiredProps.Add(paramName);
                        }
                    }
                }
            }
            else if (paramType == typeof(List<Microsoft.AspNetCore.Http.IFormFile>))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "array",
                    Items = new Microsoft.OpenApi.Models.OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    },
                    Description = param.ModelMetadata?.Description
                };
                // Lists are typically optional unless explicitly marked
            }
            else if (paramType == typeof(string))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Description = param.ModelMetadata?.Description
                };
                if (param.ParameterDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor controllerParam)
                {
                    if (!controllerParam.ParameterInfo.IsOptional && !controllerParam.ParameterInfo.HasDefaultValue)
                    {
                        requiredProps.Add(paramName);
                    }
                }
            }
            else if (paramType == typeof(int))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "integer",
                    Format = "int32",
                    Description = param.ModelMetadata?.Description
                };
                if (param.ParameterDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor controllerParam)
                {
                    if (!controllerParam.ParameterInfo.IsOptional && !controllerParam.ParameterInfo.HasDefaultValue)
                    {
                        requiredProps.Add(paramName);
                    }
                }
            }
            else if (paramType == typeof(int?))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "integer",
                    Format = "int32",
                    Nullable = true,
                    Description = param.ModelMetadata?.Description
                };
            }
            else if (paramType == typeof(bool))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "boolean",
                    Description = param.ModelMetadata?.Description
                };
                if (param.ParameterDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor controllerParam)
                {
                    if (!controllerParam.ParameterInfo.IsOptional && !controllerParam.ParameterInfo.HasDefaultValue)
                    {
                        requiredProps.Add(paramName);
                    }
                }
            }
            else if (paramType == typeof(bool?))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "boolean",
                    Nullable = true,
                    Description = param.ModelMetadata?.Description
                };
            }
            else if (paramType == typeof(DateTime) || paramType == typeof(DateTime?))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "date-time",
                    Nullable = paramType == typeof(DateTime?),
                    Description = param.ModelMetadata?.Description
                };
            }
            else if (paramType == typeof(List<string>))
            {
                properties[paramName] = new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "array",
                    Items = new Microsoft.OpenApi.Models.OpenApiSchema
                    {
                        Type = "string"
                    },
                    Description = param.ModelMetadata?.Description
                };
            }
        }

        operation.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody
        {
            Required = requiredProps.Any(),
            Content = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType>
            {
                ["multipart/form-data"] = new Microsoft.OpenApi.Models.OpenApiMediaType
                {
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties,
                        Required = requiredProps
                    }
                }
            }
        };
    }
}