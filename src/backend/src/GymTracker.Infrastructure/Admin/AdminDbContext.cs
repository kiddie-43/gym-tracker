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
    public DbSet<Domain.Entities.MonthlyPlan> MonthlyPlans => Set<Domain.Entities.MonthlyPlan>();
    public DbSet<PlanWeek> PlanWeeks => Set<PlanWeek>();
    public DbSet<PlanDay> PlanDays => Set<PlanDay>();
    public DbSet<PlannedExercise> PlannedExercises => Set<PlannedExercise>();
    public DbSet<HistoricalExerciseRecord> HistoricalExerciseRecords => Set<HistoricalExerciseRecord>();
    public DbSet<Domain.Entities.TrainingSession> TrainingSessions => Set<Domain.Entities.TrainingSession>();
    public DbSet<SessionBlock> SessionBlocks => Set<SessionBlock>();
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
        ConfigureMonthlyPlan(modelBuilder);
        ConfigurePlanWeek(modelBuilder);
        ConfigurePlanDay(modelBuilder);
        ConfigurePlannedExercise(modelBuilder);
        ConfigureHistoricalExerciseRecord(modelBuilder);
        ConfigureTrainingSession(modelBuilder);
        ConfigureSessionBlock(modelBuilder);
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

            entity.Property(x => x.WeekNumber)
                .IsRequired();

            entity.Property(x => x.DayNumber)
                .IsRequired();

            entity.Property(x => x.ExerciseCode)
                .IsRequired();

            entity.Property(x => x.UnitCode)
                .IsRequired();

            entity.Property(x => x.Value)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.Timestamp)
                .IsRequired();

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.HasIndex(x => x.GroupId);
            entity.HasIndex(x => x.WeekNumber);
            entity.HasIndex(x => x.DayNumber);
            entity.HasIndex(x => x.ExerciseCode);
            entity.HasIndex(x => x.UnitCode);
            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => new
            {
                x.WeekNumber,
                x.DayNumber,
                x.ExerciseCode
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

            entity.Property(e => e.ExerciseType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

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

    private static void ConfigureMonthlyPlan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.MonthlyPlan>(entity =>
        {
            entity.ToTable("MonthlyPlans");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.ActiveDays)
                .IsRequired();

            entity.Property(e => e.MigrationVersion)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasMany(e => e.Weeks)
                .WithOne()
                .HasForeignKey(e => e.MonthlyPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.PlannedExercises)
                .WithOne()
                .HasForeignKey(e => e.MonthlyPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId)
                .HasFilter("[DeletedAt] IS NULL")
                .IsUnique();
        });
    }

    private static void ConfigurePlanWeek(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanWeek>(entity =>
        {
            entity.ToTable("PlanWeeks");

            entity.HasKey(e => new
            {
                e.MonthlyPlanId,
                e.WeekNumber
            });

            entity.Property(e => e.WeekNumber)
                .IsRequired();

            entity.HasMany(e => e.Days)
                .WithOne()
                .HasForeignKey(e => new
                {
                    e.MonthlyPlanId,
                    e.WeekNumber
                })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePlanDay(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanDay>(entity =>
        {
            entity.ToTable("PlanDays");

            entity.HasKey(e => new
            {
                e.MonthlyPlanId,
                e.WeekNumber,
                e.DayNumber
            });

            entity.Property(e => e.DayNumber)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.TruncatedAt);
        });
    }

    private static void ConfigurePlannedExercise(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlannedExercise>(entity =>
        {
            entity.ToTable("PlannedExercises");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.MonthlyPlanId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.WeekNumber)
                .IsRequired();

            entity.Property(e => e.DayNumber)
                .IsRequired();

            entity.Property(e => e.ExerciseId)
                .IsRequired();

            entity.Property(e => e.OrderIndex)
                .IsRequired();

            entity.HasIndex(e => new
            {
                e.MonthlyPlanId,
                e.WeekNumber,
                e.DayNumber,
                e.OrderIndex
            });

            entity.HasIndex(e => e.UserId);

            entity.HasOne<PlanDay>()
                .WithMany()
                .HasForeignKey(e => new
                {
                    e.MonthlyPlanId,
                    e.WeekNumber,
                    e.DayNumber
                })
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<Exercice>()
                .WithMany()
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureHistoricalExerciseRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistoricalExerciseRecord>(entity =>
        {
            entity.ToTable("HistoricalExerciseRecords");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.SourcePlannedExerciseId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.WeekNumber)
                .IsRequired();

            entity.Property(e => e.DayNumber)
                .IsRequired();

            entity.Property(e => e.Payload)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(e => e.SourceReason)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.RecordedAt)
                .IsRequired();
        });
    }

    private static void ConfigureTrainingSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.TrainingSession>(entity =>
        {
            entity.ToTable("TrainingSessions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.WeekNumber)
                .IsRequired();

            entity.Property(e => e.DayNumber)
                .IsRequired();

            entity.Property(e => e.ExerciseId)
                .IsRequired();

            entity.Property(e => e.Timestamp)
                .IsRequired();

            entity.Property(e => e.DurationMinutes)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.SecondaryMetricValue)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.SecondaryMetricUnitCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.TertiaryMetricValue)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Notes)
                .HasMaxLength(150);

            entity.HasMany(e => e.Blocks)
                .WithOne()
                .HasForeignKey(e => e.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Exercice>()
                .WithMany()
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new
            {
                e.UserId,
                e.WeekNumber,
                e.DayNumber,
                e.ExerciseId
            });
        });
    }

    private static void ConfigureSessionBlock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionBlock>(entity =>
        {
            entity.ToTable("SessionBlocks");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.TrainingSessionId)
                .IsRequired();

            entity.Property(e => e.BlockType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.DurationValue)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.DurationUnitCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(100);

            entity.Property(e => e.OrderIndex)
                .IsRequired();

            entity.HasIndex(e => e.TrainingSessionId);
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