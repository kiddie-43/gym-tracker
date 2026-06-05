using System.Text.Json;
using GymTracker.Domain.Common;
using GymTracker.Domain.Entities;

using DomainUnit = GymTracker.Domain.Entities.Units;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymTracker.Infrastructure.Admin;

public sealed class AdminDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new();


    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<DomainUnit> Units => Set<DomainUnit>();
    public DbSet<TrainingLog> TrainingLogs => Set<TrainingLog>();
    public DbSet<Example> Examples => Set<Example>();
    public DbSet<Exercice> Exercices => Set<Exercice>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureMuscle(modelBuilder);
        ConfigureUnits(modelBuilder);
        ConfigureExample(modelBuilder);
        ConfigureTrainingLog(modelBuilder);
        ConfigureAuditableEntities(modelBuilder);
        ConfigureExercice(modelBuilder);
        ConfigureExerciceMuscle(modelBuilder);
        ConfigureExerciceUnit(modelBuilder);
    }

    private static void ConfigureMuscle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Muscle>(entity =>
        {
            entity.ToTable("Muscles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.Code)
                .HasFilter("[DeletedAt] IS NULL")
                .IsUnique();
        });
    }

    private static void ConfigureUnits(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DomainUnit>(entity =>
        {
            entity.ToTable("Units");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.HasIndex(e => e.Code)
                .HasDatabaseName("IX_Units_Code");
        });
    }


    private static void ConfigureExample(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Example>(entity =>
        {
            entity.ToTable("Examples");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.HasIndex(e => e.Code)
                .IsUnique();
        });
    }

    private static void ConfigureTrainingLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrainingLog>(entity =>
        {
            entity.ToTable("TrainingLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.GroupId)
                .IsRequired();

            entity.Property(x => x.RoutineId)
                .IsRequired();

            entity.Property(x => x.SessionId)
                .IsRequired();

            entity.Property(x => x.ExerciseId)
                .IsRequired();

            entity.Property(x => x.MetricId)
                .IsRequired();

            entity.Property(x => x.Value)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.Timestamp)
                .IsRequired();

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.HasOne<DomainUnit>()
                .WithMany()
                .HasForeignKey(x => x.MetricId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.GroupId);
            entity.HasIndex(x => x.RoutineId);
            entity.HasIndex(x => x.SessionId);
            entity.HasIndex(x => x.ExerciseId);
            entity.HasIndex(x => x.MetricId);
            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => new
            {
                x.SessionId,
                x.ExerciseId
            });
        });
    }
    private static void ConfigureExercice(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercice>(entity =>
        {
            entity.ToTable("Exercices");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.Code)
                .HasFilter("[DeletedAt] IS NULL")
                .IsUnique();
        });
    }
    private static void ConfigureExerciceMuscle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciceMuscle>(entity =>
        {
            entity.ToTable("ExerciceMuscles");

            entity.HasKey(e => new
            {
                e.ExerciceId,
                e.MuscleId,
                e.Type
            });

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.Exercice)
                .WithMany(e => e.Muscles)
                .HasForeignKey(e => e.ExerciceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Muscle)
                .WithMany()
                .HasForeignKey(e => e.MuscleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    private static void ConfigureExerciceUnit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciceUnit>(entity =>
        {
            entity.ToTable("ExerciceUnits");

            entity.HasKey(e => new
            {
                e.ExerciceId,
                e.UnitId
            });

            entity.HasOne(e => e.Exercice)
                .WithMany(e => e.Units)
                .HasForeignKey(e => e.ExerciceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    private static void ConfigureAuditableEntities(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var entity = modelBuilder.Entity(entityType.ClrType);

            entity.Property(nameof(AuditableEntity.CreatedAt))
                .IsRequired();

            entity.Property(nameof(AuditableEntity.CreatedBy));

            entity.Property(nameof(AuditableEntity.UpdatedAt));
            entity.Property(nameof(AuditableEntity.UpdatedBy));

            entity.Property(nameof(AuditableEntity.DeletedAt));
            entity.Property(nameof(AuditableEntity.DeletedBy));

            entity.Ignore(nameof(AuditableEntity.IsDeleted));

            entity.HasIndex(nameof(AuditableEntity.DeletedAt));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = now;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Property(nameof(AuditableEntity.DeletedAt)).CurrentValue = now;
            }
        }
    }
}