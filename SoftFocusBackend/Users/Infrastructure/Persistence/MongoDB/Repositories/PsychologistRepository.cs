using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

public class PsychologistRepository : BaseRepository<PsychologistUser>, IPsychologistRepository
{
    public PsychologistRepository(MongoDbContext context) : base(context, "psychologists")
    {
    }

    public async Task<PsychologistUser?> FindByInvitationCodeAsync(string invitationCode)
    {
        var normalizedCode = invitationCode.ToUpperInvariant();
        return await Collection.Find(p => p.InvitationCode == normalizedCode && p.IsVerified).FirstOrDefaultAsync();
    }

    public async Task<PsychologistUser?> FindByLicenseNumberAsync(string licenseNumber)
    {
        return await Collection.Find(p => p.LicenseNumber == licenseNumber).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsWithLicenseNumberAsync(string licenseNumber, string? excludeUserId = null)
    {
        var filter = Builders<PsychologistUser>.Filter.Eq(p => p.LicenseNumber, licenseNumber);
        
        if (!string.IsNullOrWhiteSpace(excludeUserId))
        {
            filter = Builders<PsychologistUser>.Filter.And(filter, 
                Builders<PsychologistUser>.Filter.Ne(p => p.Id, excludeUserId));
        }
        
        return await Collection.Find(filter).AnyAsync();
    }

    public async Task<(List<PsychologistUser> Psychologists, int TotalCount)> FindPsychologistsForDirectoryAsync(
        int page, int pageSize, List<PsychologySpecialty>? specialties = null, string? city = null,
        double? minRating = null, bool? isAcceptingNewPatients = null, List<string>? languages = null,
        string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
    {
        var filterBuilder = Builders<PsychologistUser>.Filter;
        var filter = filterBuilder.And(
            filterBuilder.Eq(p => p.IsVerified, true),
            filterBuilder.Eq(p => p.IsActive, true),
            filterBuilder.Eq(p => p.IsProfileVisibleInDirectory, true)
        );

        if (specialties?.Count > 0)
        {
            filter = filterBuilder.And(filter, filterBuilder.AnyIn(p => p.Specialties, specialties));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(p => p.City, city));
        }

        if (minRating.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Gte(p => p.AverageRating, minRating.Value));
        }

        if (isAcceptingNewPatients.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(p => p.IsAcceptingNewPatients, isAcceptingNewPatients.Value));
        }

        if (languages?.Count > 0)
        {
            filter = filterBuilder.And(filter, filterBuilder.AnyIn(p => p.Languages, languages));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Where(p => p.FullName.ToLower().Contains(searchTerm.ToLower())),
                filterBuilder.Where(p => p.ProfessionalBio != null && p.ProfessionalBio.ToLower().Contains(searchTerm.ToLower())),
                filterBuilder.AnyEq<PsychologySpecialty>(p => p.Specialties, 
                    Enum.GetValues<PsychologySpecialty>()
                        .FirstOrDefault(s => s.ToString().ToLower().Contains(searchTerm.ToLower())))
            );
            filter = filterBuilder.And(filter, searchFilter);
        }

        var totalCount = await Collection.CountDocumentsAsync(filter);

        var sortDefinition = CreateSortDefinition(sortBy, sortDescending);
        var skip = (page - 1) * pageSize;

        var psychologists = await Collection.Find(filter)
            .Sort(sortDefinition)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();

        return (psychologists, (int)totalCount);
    }

    public async Task<List<PsychologistUser>> FindVerifiedPsychologistsAsync()
    {
        var filter = Builders<PsychologistUser>.Filter.And(
            Builders<PsychologistUser>.Filter.Eq(p => p.IsVerified, true),
            Builders<PsychologistUser>.Filter.Eq(p => p.IsActive, true)
        );
        
        return await Collection.Find(filter).ToListAsync();
    }

    public async Task<List<PsychologistUser>> FindPendingVerificationAsync()
    {
        var filter = Builders<PsychologistUser>.Filter.And(
            Builders<PsychologistUser>.Filter.Eq(p => p.IsVerified, false),
            Builders<PsychologistUser>.Filter.Eq(p => p.IsActive, true),
            Builders<PsychologistUser>.Filter.Ne(p => p.LicenseNumber, string.Empty)
        );
        
        return await Collection.Find(filter).ToListAsync();
    }

    public async Task<List<PsychologistUser>> FindWithExpiredCodesAsync()
    {
        var now = DateTime.UtcNow;
        var filter = Builders<PsychologistUser>.Filter.And(
            Builders<PsychologistUser>.Filter.Eq(p => p.IsVerified, true),
            Builders<PsychologistUser>.Filter.Lt(p => p.InvitationCodeExpiresAt, now)
        );
        
        return await Collection.Find(filter).ToListAsync();
    }

    public async Task RegenerateExpiredCodesAsync()
    {
        var expiredPsychologists = await FindWithExpiredCodesAsync();
        
        foreach (var psychologist in expiredPsychologists)
        {
            psychologist.GenerateNewInvitationCode();
            await Collection.ReplaceOneAsync(p => p.Id == psychologist.Id, psychologist);
        }
    }

    private static SortDefinition<PsychologistUser> CreateSortDefinition(string? sortBy, bool sortDescending)
    {
        var sortBuilder = Builders<PsychologistUser>.Sort;
        
        var sortField = sortBy?.ToLowerInvariant() switch
        {
            "fullname" => sortDescending ? sortBuilder.Descending(p => p.FullName) : sortBuilder.Ascending(p => p.FullName),
            "rating" => sortDescending ? sortBuilder.Descending(p => p.AverageRating) : sortBuilder.Ascending(p => p.AverageRating),
            "experience" => sortDescending ? sortBuilder.Descending(p => p.YearsOfExperience) : sortBuilder.Ascending(p => p.YearsOfExperience),
            "city" => sortDescending ? sortBuilder.Descending(p => p.City) : sortBuilder.Ascending(p => p.City),
            "createdat" => sortDescending ? sortBuilder.Descending(p => p.CreatedAt) : sortBuilder.Ascending(p => p.CreatedAt),
            _ => sortDescending ? sortBuilder.Descending(p => p.AverageRating) : sortBuilder.Ascending(p => p.AverageRating)
        };

        return sortField;
    }
}