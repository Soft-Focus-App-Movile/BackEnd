# Database Seeder

Este archivo contiene el seeder de la base de datos que se ejecuta automáticamente al iniciar la aplicación en modo Development.

## Uso

El seeder se ejecuta automáticamente cuando inicias la aplicación en modo Development. Solo crea data si la colección está vacía (excepto el admin).

## Cómo agregar data de otros Bounded Contexts

Para agregar data de seeding de otros bounded contexts, sigue estos pasos:

### 1. Crear método privado en DatabaseSeeder.cs

```csharp
private async Task SeedTuContextoAsync()
{
    var collection = _context.Database.GetCollection<TuEntidad>("tuColeccion");
    var count = await collection.CountDocumentsAsync(FilterDefinition<TuEntidad>.Empty);

    if (count > 0)
    {
        _logger.LogInformation("Tu data ya existe. Skipping seeding.");
        return;
    }

    _logger.LogInformation("Seeding tu contexto...");

    var items = new List<TuEntidad>
    {
        new TuEntidad
        {
            // Tus datos aquí
        }
    };

    await collection.InsertManyAsync(items);
    _logger.LogInformation($"Seeded {items.Count} items.");
}
```

### 2. Llamar el método en SeedAsync()

```csharp
public async Task SeedAsync()
{
    _logger.LogInformation("Starting database seeding...");

    await SeedUsersAsync();
    await SeedTuContextoAsync(); // Agregar aquí

    _logger.LogInformation("Database seeding completed.");
}
```

## Data actual seeded

### Users (5 usuarios generales)
- **patient1@test.com** - Laura Gomez - Password: `Patient123!`
- **patient2@test.com** - Carlos Martinez - Password: `Patient123!`
- **patient3@test.com** - Ana Garcia - Password: `Patient123!`
- **patient4@test.com** - Luis Torres - Password: `Patient123!`
- **patient5@test.com** - Maria Lopes - Password: `Patient123!`

### Psychologists (3 psicólogos verificados)
- **psychologist1@test.com** - Dra. Patricia Sanchez - Password: `Psy123!`
- **psychologist2@test.com** - Dr. Ramiro Miranda Loza - Password: `Psy123!`
- **psychologist3@test.com** - Dra. Sofia Ramirez - Password: `Psy123!`

## Notas

- El seeder solo se ejecuta en **Development mode**
- Solo crea data si la colección tiene menos de 2 documentos (para evitar duplicados)
- Los passwords están hasheados con BCrypt
- Todos los psicólogos están verificados (`IsVerified = true`)
- El admin (`admin@softfocus.com`) se crea aparte en Program.cs

## Ejemplo: Agregar Therapeutic Relationships

```csharp
private async Task SeedTherapeuticRelationshipsAsync()
{
    var collection = _context.Database.GetCollection<TherapeuticRelationship>("therapeuticRelationships");
    var count = await collection.CountDocumentsAsync(FilterDefinition<TherapeuticRelationship>.Empty);

    if (count > 0)
    {
        _logger.LogInformation("Therapeutic relationships already exist. Skipping.");
        return;
    }

    var usersCollection = _context.Database.GetCollection<User>("users");
    var patient1 = await usersCollection.Find(u => u.Email == "patient1@test.com").FirstOrDefaultAsync();
    var psychologist1 = await usersCollection.Find(u => u.Email == "psychologist1@test.com").FirstOrDefaultAsync();

    if (patient1 == null || psychologist1 == null)
    {
        _logger.LogWarning("Required users not found for seeding relationships");
        return;
    }

    var relationships = new List<TherapeuticRelationship>
    {
        new TherapeuticRelationship
        {
            PatientId = patient1.Id,
            PsychologistId = psychologist1.Id,
            Status = RelationshipStatus.Active,
            CreatedAt = DateTime.UtcNow
        }
    };

    await collection.InsertManyAsync(relationships);
    _logger.LogInformation($"Seeded {relationships.Count} therapeutic relationships.");
}
```
