  # CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SoftFocus is a .NET 9 backend API for a mental health support platform that connects users with psychologists. The application uses MongoDB for persistence, JWT for authentication, OAuth (Google/Facebook) for social login, and integrates with external services (Cloudinary for images, SMTP for email).

## Common Commands

### Development
```bash
# Restore dependencies
dotnet restore SoftFocusBackend/SoftFocusBackend.csproj

# Build the project
dotnet build SoftFocusBackend/SoftFocusBackend.csproj

# Run the application locally
dotnet run --project SoftFocusBackend/SoftFocusBackend.csproj

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Development dotnet run --project SoftFocusBackend/SoftFocusBackend.csproj
```

### Docker Deployment
```bash
# Build and start all services (MongoDB + Backend)
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f softfocus-dotnet

# Rebuild and restart
docker-compose up -d --build
```

### Database
```bash
# MongoDB runs on port 27017
# Connection requires authentication (set in .env file)
# Database: softfocus_db
# User: softfocus_user (read/write permissions)
```

## Architecture & Design Patterns

### Domain-Driven Design (DDD) Structure
The codebase follows DDD with bounded contexts organized by feature:

**Bounded Contexts:**
- `Auth/` - Authentication, JWT tokens, OAuth providers, password reset
- `Users/` - User management, psychologist profiles, verification
- `Shared/` - Cross-cutting concerns (MongoDB infrastructure, email, Cloudinary)

**Layer Organization (per bounded context):**
- `Domain/` - Core business logic, aggregates, value objects, commands, queries, domain services
- `Application/` - Application services (CommandServices, QueryServices), facades (ACL), outbound services
- `Infrastructure/` - Data persistence (MongoDB repositories), external services, ACL implementations
- `Interfaces/REST/` - Controllers, resources (DTOs), transformers/assemblers

### Key Patterns

**CQRS (Command Query Responsibility Segregation):**
- Commands: Write operations defined in `Domain/Model/Commands/`
- Queries: Read operations defined in `Domain/Model/Queries/`
- Services: Separate command and query services (e.g., `IAuthCommandService`, `IAuthQueryService`)

**Repository Pattern:**
- Generic `IBaseRepository<T>` in `Shared/Domain/Repositories/`
- Specific repositories like `IUserRepository`, `IPsychologistRepository`
- MongoDB implementation in `Infrastructure/Persistence/MongoDB/Repositories/`

**Unit of Work:**
- `IUnitOfWork` coordinates transactions across repositories
- Implementation in `Shared/Infrastructure/Persistence/MongoDB/Repositories/UnitOfWork.cs`

**Facade Pattern (ACL - Anti-Corruption Layer):**
- Facades expose bounded context functionality to other contexts
- Example: `IAuthFacade`, `IUserFacade`
- ACL services coordinate cross-context communication (e.g., `IUserContextService` in Auth calls User context)

### MongoDB Integration

**Context:**
- `MongoDbContext` (Shared/Infrastructure/Persistence/MongoDB/Context/) provides database access
- Configured via `MongoDbSettings` from appsettings.json
- Connection pooling, timeouts configured in settings

**Entities:**
- All entities inherit from `BaseEntity` (contains Id, CreatedAt, UpdatedAt)
- Use `[BsonElement]` attributes for field mapping
- Collections: "users", "psychologists"

**Collections:**
- Users: Base user aggregate (email, password, profile, preferences)
- Psychologists: Extends user concept with professional data (license, specialties, verification status)

### Authentication & Authorization

**JWT Tokens:**
- `TokenService` generates and validates JWT tokens
- Settings in `TokenSettings` (secret, issuer, audience, expiration)
- Claims include userId, email, userType, fullName

**OAuth Providers:**
- Google: `GoogleOAuthService` - validates Google tokens
- Facebook: `FacebookOAuthService` - validates Facebook tokens
- Interface: `IOAuthService` for provider abstraction

**User Types:**
- General: Regular users seeking mental health support
- Psychologist: Mental health professionals (require verification)
- Admin: System administrators

### External Services

**Cloudinary (Image Storage):**
- `ICloudinaryImageService` handles profile image uploads
- Settings: CloudName, ApiKey, ApiSecret, ProfileImagesFolder
- Validates file size and extensions

**Email (SMTP):**
- `IGenericEmailService` sends emails (password reset, notifications)
- Uses Gmail SMTP (configurable)
- Settings: SmtpServer, SmtpPort, FromEmail, SmtpUser, SmtpPassword

## Configuration

### Environment Variables (.env file required)
```
# MongoDB
MONGO_ROOT_PASSWORD=<admin_password>
MONGO_USER_PASSWORD=<app_user_password>
MongoDbSettings__ConnectionString=mongodb://softfocus_user:<password>@mongodb:27017/softfocus_db?authSource=softfocus_db

# JWT
TokenSettings__SecretKey=<jwt_secret_key>

# Email
EmailSettings__FromEmail=<email>
EmailSettings__SmtpUser=<email>
EmailSettings__SmtpPassword=<app_password>

# Cloudinary
CloudinarySettings__CloudName=<cloud_name>
CloudinarySettings__ApiKey=<api_key>
CloudinarySettings__ApiSecret=<api_secret>

# OAuth
GoogleOAuthSettings__ClientId=<google_client_id>
GoogleOAuthSettings__ClientSecret=<google_client_secret>
FacebookOAuthSettings__AppId=<facebook_app_id>
FacebookOAuthSettings__AppSecret=<facebook_app_secret>
```

### API Configuration
- CORS: Allows localhost:5173, localhost:8080, localhost:3000, softfocus.netlify.app
- Routes: Kebab-case convention (e.g., `/api/v1/auth/forgot-password`)
- Swagger: Available at `/swagger` endpoint

## Important Notes

### User Registration Flow
1. `AuthController.Register()` receives registration request
2. Uses `IUserContextService.CreateUserAsync()` (ACL to Users context)
3. For psychologists: Creates entry in both users and psychologists collections
4. Returns userId on success (does NOT return JWT token on registration)

### Psychologist Verification
- Psychologists require professional verification before full access
- `IPsychologistVerificationService` handles verification logic
- Invitation codes used for controlled psychologist onboarding
- Admin endpoints in `AdminController` for verification management

### MongoDB Collections
- Collection names are pluralized lowercase (e.g., "users", "psychologists")
- Indexes: Unique on email, index on userType
- BaseEntity provides common fields: Id (string/ObjectId), CreatedAt, UpdatedAt

### Dependency Injection (Program.cs)
- All services registered in `Program.cs`
- Scoped lifetime for most services (per request)
- Singleton for `MongoDbContext`
- HttpClient configured for OAuth services

### Error Handling
- Controllers catch exceptions and return appropriate HTTP status codes
- Consistent error responses via `AuthResourceAssembler.ToErrorResponse()`
- Logging via `ILogger<T>` throughout controllers and services

## Main Branch & Deployment

- Main branch: `main`
- Current feature branch pattern: `feature/<name>` (e.g., feature/usersBC)
- Docker deployment: Builds multi-stage Dockerfile, exposes port 5000 (maps to internal 8080)
- Database auto-resets on deploy (see recent commits)