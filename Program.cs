using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("OncallDb") ??
                       "User ID=postgres;Password=1234;Host=localhost;Port=5432;Database=oncall2;Pooling=true;Maximum Pool Size=10";
builder.Services
    .AddDbContext<OncallDb>(options => options.AddInterceptors(new AuditingInterceptor()).UseNpgsql(connectionString))
    .AddDatabaseDeveloperPageExceptionFilter();
builder.Services
    .AddDbContext<VerticalDb>(options =>
        options.UseNpgsql(
            "User ID=postgres;Password=1234;Host=localhost;Port=5432;Database=audit;Pooling=true;Maximum Pool Size=10"))
    .AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await EnsureDb(app.Services, app.Logger);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.MapGet("/error", () => Results.Problem("An error occurred.", statusCode: 500))
    .ExcludeFromDescription();

app.MapSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!")
    .WithName("Hello");

app.MapGet("/hello", () => new { Hello = "World" })
    .WithName("HelloObject");

app.MapPost("/addVirusObj", async (OncallDb db) =>
    {
        var maybeUser = await db.Users.FirstOrDefaultAsync(p => p.FullName == "Adam Smith");
        if (maybeUser == null) return Results.NotFound();
        var admin = new Admin(maybeUser.FullName, maybeUser.TimeZone, maybeUser.PhotoUrl);
        db.Admins.Add(admin);
        var team = await db.Teams.FirstOrDefaultAsync(p => p.Name == "Monitoring");
        if (team == null) return Results.NotFound();
        admin.Administrate.Add(team);
        await db.SaveChangesAsync();
        return Results.Ok("Virus object added");
    })
    .WithName("Simulate_on_oncall_DB");

app.MapGet("/inspectClassic", async (OncallDb db) =>
{
    var team = await db.Teams
        .Include(p => p.Administrate)
        .Include(d => d.Duty)
        .FirstOrDefaultAsync(p => p.Name == "Monitoring");
    if (team == null) return Results.NotFound();
    var result = team.Administrate.Any(p => p.FullName == "Adam Smith") &&
                 team.Duty.Any(p => p.FullName == "Adam Smith");
    return Results.Ok(result);
});

app.MapGet("/inspectVertical", async (VerticalDb db) =>
{
    var individs = await db.Individs.Include(p => p.Values).ThenInclude(p=>p.Attribute).ThenInclude(p=>p.Concept).ToListAsync();
    var maybeImpostor=individs.FirstOrDefault(p =>
        p.Values.Exists(x =>
            x.Attribute.Concept.Name == "TeamUser (Dictionary<string, object>)" &&
            x.Attribute.Name == "DutyFullName") && p.Values.Exists(x =>
            x.Attribute.Concept.Name == "AdminTeam (Dictionary<string, object>)" &&
            x.Attribute.Name == "AdministrateFullName"));
    return maybeImpostor == null ? Results.NotFound() : Results.Ok($"Individ {maybeImpostor.Id} virus! BelongsTo Duty and Administrate concepts");
});

app.MapDelete("/clearVirus", async (OncallDb db) =>
    {
        var maybeAdmin = await db.Admins.FirstOrDefaultAsync(p => p.FullName == "Adam Smith");
        if (maybeAdmin == null) return Results.NotFound();
        var maybeUser = await db.Users.FirstOrDefaultAsync(p => p.FullName == "Adam Smith");
        if (maybeUser == null) return Results.NotFound();
        var maybeTeam = await db.Teams.FirstOrDefaultAsync(p => p.Name == "Monitoring");
        if (maybeTeam == null) return Results.NotFound();
        db.Remove(maybeTeam);
        db.Remove(maybeAdmin);
        db.Remove(maybeUser);
        await db.SaveChangesAsync();
        return Results.NoContent();
        ;
    })
    .WithName("Clear");

app.MapPost("/addAdam", async (OncallDb db) =>
{
    var user = new User("Adam Smith", null, null);
    db.Users.Add(user);
    var team = db.Teams.Add(new Team("Monitoring", null, null, null, null));
    user.Duty.Add(team.Entity);
    await db.SaveChangesAsync();
    return Results.Ok("Adam object added");
});
app.MapDelete("/delete-allAudit", async (VerticalDb db) =>
        Results.Ok(await db.Database.ExecuteSqlRawAsync("TRUNCATE \"Concepts\" CASCADE")))
    .WithName("DeleteAll");

app.Run();

async Task EnsureDb(IServiceProvider services, ILogger logger)
{
    logger.LogInformation("Ensuring database exists and is up to date at connection string '{connectionString}'",
        connectionString);

    await using var db = services.CreateScope().ServiceProvider.GetRequiredService<OncallDb>();
    await db.Database.MigrateAsync();
    await using var db2 = services.CreateScope().ServiceProvider.GetRequiredService<VerticalDb>();
    await db2.Database.MigrateAsync();
}

public record ContactMode(string Name);

public record UserContact(
    string Destination
)
{
    public User User { get; set; }
    public ContactMode ContactMode { get; set; }
}

public record Role(
    string Name,
    int DisplayOrder = 1
);

public record Scheduler(
    string Name,
    string Description
);

public record Schedule(
    int Id,
    int AutoPopulateThreshold,
    long LastEpochScheduled,
    bool AdvancedMode = false
)
{
    public Scheduler Scheduler { get; set; }
    public Team Team { get; set; }
    public Role Role { get; set; }
    public Roster Roster { get; set; }
}

public record Event(
    long Id,
    long Start,
    long End,
    string? Note
)
{
    public Schedule Schedule { get; set; }
    public Team Team { get; set; }
    public Role Role { get; set; }
    public User User { get; set; }
}

public record User(
    string FullName,
    string? TimeZone,
    string? PhotoUrl,
    bool Active = true
)
{
    public List<Team> Duty { get; } = new();
    public List<UserContact> UserContacts { get; } = new();
    public List<RosterUser> Rotates { get; } = new();
}

public record Admin(
    string FullName,
    string? TimeZone,
    string? PhotoUrl,
    bool Active = true,
    bool God = false
)
{
    public List<Team> Administrate { get; } = new();
}

public record Roster(
    string Name
)
{
    public Team Team { get; set; }
}

public record RosterUser(int RosterPriority, bool InRotation = true)
{
    public User User { get; set; }
    public Roster Roster { get; set; }
}

public record Team(
    string Name,
    string? SlackChannel,
    string? SlackChannelNotifications,
    string? Email,
    string? SchedulingTimezone,
    bool Active = true
)
{
    public List<User> Duty { get; } = new();
    public List<Admin> Administrate { get; } = new();
    public List<Roster> Rotates { get; } = new();
}

class OncallDb : DbContext
{
    public OncallDb(DbContextOptions<OncallDb> options)
        : base(options)
    {
    }

    public DbSet<ContactMode> ContactModes => Set<ContactMode>();
    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<UserContact> UserContacts { get; set; }
    public DbSet<Roster> Rosters { get; set; }
    public DbSet<RosterUser> RosterUsers { get; set; }
    public DbSet<Scheduler> Schedulers { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>(mode => mode.HasKey(p => p.Name));
        modelBuilder.Entity<Scheduler>(mode => mode.HasKey(p => p.Name));
        modelBuilder.Entity<ContactMode>(mode => mode.HasKey(p => p.Name));
        modelBuilder.Entity<User>(user => user.HasKey(p => p.FullName));
        modelBuilder.Entity<Admin>(admin => admin.HasKey(t => t.FullName));
        modelBuilder.Entity<Roster>(admin => admin.HasKey(t => t.Name));
        modelBuilder.Entity<Team>(
            team =>
            {
                team.HasKey(d => d.Name);
                team.HasMany(d => d.Duty).WithMany(s => s.Duty);
                team.HasMany(d => d.Administrate).WithMany(t => t.Administrate);
            }
        );
        modelBuilder.Entity<UserContact>(mode => { mode.HasKey("UserFullName", "ContactModeName"); });
        modelBuilder.Entity<UserContact>()
            .HasOne(p => p.User)
            .WithMany(p => p.UserContacts)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RosterUser>(mode => { mode.HasKey("UserFullName", "RosterName"); });
        modelBuilder.Entity<RosterUser>()
            .HasOne(p => p.User)
            .WithMany(p => p.Rotates)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public record Concept(string Name)
{
    public List<Attribute> Attributes { get; } = new();
}

public record Attribute(
    string Name,
    DateTime ActualFrom,
    DateTime? ActualTo)
{
    public Concept Concept { get; set; }
    public List<AttributeValue> Values { get; } = new();
}

public record Individ(int Id)
{
    public List<AttributeValue> Values { get; } = new();
}

public record AttributeValue(
    int Id,
    JsonDocument Value,
    DateTime ActualFrom,
    DateTime? ActualTo)
{
    public Attribute Attribute { get; set; }
    public Individ Individ { get; set; }
}

class VerticalDb : DbContext
{
    public VerticalDb(DbContextOptions<VerticalDb> options)
        : base(options)
    {
    }

    public DbSet<Concept> Concepts { get; set; }
    public DbSet<Attribute> Attributes { get; set; }
    public DbSet<Individ> Individs { get; set; }
    public DbSet<AttributeValue> AttributeValues { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Concept>(mode => mode.HasKey(p => p.Name));
        modelBuilder.Entity<Attribute>(mode => mode.HasKey("Name", "ConceptName", "ActualFrom"));

        modelBuilder.Entity<AttributeValue>(mode => { mode.HasKey("IndividId", "AttributeName", "ActualFrom"); });
        modelBuilder.Entity<AttributeValue>()
            .HasOne(p => p.Attribute)
            .WithMany(p => p.Values)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AttributeValue>()
            .HasOne(p => p.Individ)
            .WithMany(p => p.Values)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditingInterceptor : ISaveChangesInterceptor
{
    private readonly DbContextOptions<VerticalDb> _options;

    public AuditingInterceptor()
    {
        _options = new DbContextOptionsBuilder<VerticalDb>()
            .UseNpgsql(
                "User ID=postgres;Password=1234;Host=localhost;Port=5432;Database=audit;Pooling=true;Maximum Pool Size=10")
            .Options;
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        return result;
    }

    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        return result;
    }

    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
    }

    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        eventData.Context?.ChangeTracker.DetectChanges();
        var concepts = eventData.Context?.Model.GetEntityTypes()
            .Select(p => (p.Name, Attributes: p.GetProperties().Select(x => x.Name))).ToList();

        await using var auditContext = new VerticalDb(_options);
        foreach (var (name, attributes) in concepts)
        {
            var c = new Concept(name);
            auditContext.Concepts.Add(c);
            foreach (var atrValue in attributes)
                auditContext.Attributes.Add(new Attribute(atrValue, DateTime.UtcNow, null) { Concept = c });
        }

        try
        {
            await auditContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            auditContext.ChangeTracker.Clear();
        }

        foreach (var entry in eventData.Context?.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    foreach (var property in entry.Properties)
                    {
                        var val = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(property.CurrentValue));
                        var attributeValueVal = await auditContext
                            .AttributeValues
                            .FirstOrDefaultAsync(p =>
                                p.Value == val);
                        if (attributeValueVal == null) continue;
                        auditContext
                            .AttributeValues.Remove(attributeValueVal);
                    }

                    break;
                case EntityState.Modified:
                    foreach (var property in entry.Properties)
                    {
                        var val = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(property.OriginalValue));
                        var attributeValueVal = await auditContext
                            .AttributeValues
                            .FirstOrDefaultAsync(p =>
                                p.Value == val);
                        if (attributeValueVal == null) continue;

                        auditContext
                            .Entry(attributeValueVal).CurrentValues.SetValues(attributeValueVal with
                            {
                                Value = JsonDocument.Parse(
                                    JsonSerializer.SerializeToUtf8Bytes(property.CurrentValue))
                            });
                    }

                    break;
                case EntityState.Added:
                    var i = auditContext.Individs.Add(new Individ(0));
                    // await auditContext.SaveChangesAsync(cancellationToken);
                    var maybeConcept =
                        await auditContext.Concepts.FirstOrDefaultAsync(p => p.Name == entry.Metadata.DisplayName(),
                            cancellationToken);
                    var concept = maybeConcept ?? new Concept(entry.Metadata.DisplayName());
                    Individ? existIndivid = null;
                    foreach (var property in entry.Properties)
                    {
                        var maybeAtr = await auditContext.Attributes.Include(p => p.Concept).FirstOrDefaultAsync(
                            p => p.Name == property.Metadata.Name && p.Concept.Name == entry.Metadata.DisplayName(),
                            cancellationToken);
                        var atr = maybeAtr ?? new Attribute(property.Metadata.Name, DateTime.UtcNow, null)
                            { Concept = concept };
                        var jsDoc = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(property.CurrentValue));
                        var attributeValueVal = await auditContext
                            .AttributeValues
                            .Include(p => p.Individ)
                            .FirstOrDefaultAsync(p =>
                                p.Value == jsDoc);
                        if (attributeValueVal != null && property.Metadata.IsPrimaryKey())
                        {
                            existIndivid = attributeValueVal.Individ;
                            auditContext.AttributeValues
                                .Add(new AttributeValue(
                                        0,
                                        jsDoc,
                                        DateTime.UtcNow,
                                        null)
                                    { Individ = existIndivid, Attribute = atr });
                        }
                        else
                        {
                            auditContext.AttributeValues
                                .Add(new AttributeValue(
                                        0,
                                        jsDoc,
                                        DateTime.UtcNow,
                                        null)
                                    { Individ = existIndivid ?? i.Entity, Attribute = atr });
                        }
                    }

                    break;
                default:
                    break;
            }

            await auditContext.SaveChangesAsync(cancellationToken);
        }

        // await auditContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new())
    {
        return result;
    }

    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }
}