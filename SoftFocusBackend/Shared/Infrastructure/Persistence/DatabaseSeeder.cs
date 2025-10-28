using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using BC = BCrypt.Net.BCrypt;

namespace SoftFocusBackend.Shared.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly MongoDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(MongoDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        await SeedUsersAsync();

        _logger.LogInformation("Database seeding completed.");
    }

    private async Task SeedUsersAsync()
    {
        var usersCollection = _context.Database.GetCollection<User>("users");
        var count = await usersCollection.CountDocumentsAsync(FilterDefinition<User>.Empty);

        if (count > 1)
        {
            _logger.LogInformation("Users already exist. Skipping user seeding.");
            return;
        }

        _logger.LogInformation("Seeding users...");

        var users = new List<User>
        {
            new User
            {
                Email = "patient1@test.com",
                PasswordHash = BC.HashPassword("Patient123!"),
                UserType = UserType.General,
                FullName = "Laura Gomez",
                FirstName = "Laura",
                LastName = "Gomez",
                DateOfBirth = new DateTime(2004, 5, 15),
                Gender = "Femenino",
                Phone = "+51987654321",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Email = "patient2@test.com",
                PasswordHash = BC.HashPassword("Patient123!"),
                UserType = UserType.General,
                FullName = "Carlos Martinez",
                FirstName = "Carlos",
                LastName = "Martinez",
                DateOfBirth = new DateTime(2001, 8, 22),
                Gender = "Masculino",
                Phone = "+51987654322",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Email = "patient3@test.com",
                PasswordHash = BC.HashPassword("Patient123!"),
                UserType = UserType.General,
                FullName = "Ana Garcia",
                FirstName = "Ana",
                LastName = "Garcia",
                DateOfBirth = new DateTime(2003, 3, 10),
                Gender = "Femenino",
                Phone = "+51987654323",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Email = "patient4@test.com",
                PasswordHash = BC.HashPassword("Patient123!"),
                UserType = UserType.General,
                FullName = "Luis Torres",
                FirstName = "Luis",
                LastName = "Torres",
                DateOfBirth = new DateTime(2002, 11, 5),
                Gender = "Masculino",
                Phone = "+51987654324",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Email = "patient5@test.com",
                PasswordHash = BC.HashPassword("Patient123!"),
                UserType = UserType.General,
                FullName = "Maria Lopes",
                FirstName = "Maria",
                LastName = "Lopes",
                DateOfBirth = new DateTime(2000, 7, 18),
                Gender = "Femenino",
                Phone = "+51987654325",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var psychologists = new List<PsychologistUser>
        {
            new PsychologistUser
            {
                Email = "psychologist1@test.com",
                PasswordHash = BC.HashPassword("Psy123!"),
                UserType = UserType.Psychologist,
                FullName = "Dra. Patricia Sanchez",
                FirstName = "Patricia",
                LastName = "Sanchez",
                DateOfBirth = new DateTime(1985, 4, 12),
                Gender = "Femenino",
                Phone = "+51987654330",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                IsVerified = true,
                LicenseNumber = "PSY-2015-001234",
                ProfessionalCollege = "Colegio de Psicólogos del Perú",
                CollegeRegion = "Lima",
                Specialties = new List<PsychologySpecialty> { PsychologySpecialty.Clinica },
                YearsOfExperience = 8,
                VerificationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PsychologistUser
            {
                Email = "psychologist2@test.com",
                PasswordHash = BC.HashPassword("Psy123!"),
                UserType = UserType.Psychologist,
                FullName = "Dr. Ramiro Miranda Loza",
                FirstName = "Ramiro",
                LastName = "Miranda Loza",
                DateOfBirth = new DateTime(1982, 9, 25),
                Gender = "Masculino",
                Phone = "+51987654331",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                IsVerified = true,
                LicenseNumber = "PSY-2012-005678",
                ProfessionalCollege = "Colegio de Psicólogos del Perú",
                CollegeRegion = "Lima",
                Specialties = new List<PsychologySpecialty> { PsychologySpecialty.Ansiedad, PsychologySpecialty.Depresion },
                YearsOfExperience = 12,
                VerificationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PsychologistUser
            {
                Email = "psychologist3@test.com",
                PasswordHash = BC.HashPassword("Psy123!"),
                UserType = UserType.Psychologist,
                FullName = "Dra. Sofia Ramirez",
                FirstName = "Sofia",
                LastName = "Ramirez",
                DateOfBirth = new DateTime(1988, 6, 8),
                Gender = "Femenino",
                Phone = "+51987654332",
                Country = "Peru",
                City = "Lima",
                IsActive = true,
                IsVerified = true,
                LicenseNumber = "PSY-2018-009876",
                ProfessionalCollege = "Colegio de Psicólogos del Perú",
                CollegeRegion = "Lima",
                Specialties = new List<PsychologySpecialty> { PsychologySpecialty.Infantil, PsychologySpecialty.Adolescentes },
                YearsOfExperience = 6,
                VerificationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await usersCollection.InsertManyAsync(users);
        await usersCollection.InsertManyAsync(psychologists);

        _logger.LogInformation($"Seeded {users.Count} general users and {psychologists.Count} psychologists.");
    }
}
