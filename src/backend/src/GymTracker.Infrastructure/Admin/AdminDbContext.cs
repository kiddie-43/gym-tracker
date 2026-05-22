using System.Text.Json;

using GymTracker.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GymTracker.Infrastructure.Admin;

public sealed class AdminDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    private static readonly ValueConverter<IReadOnlyCollection<string>, string> StringCollectionConverter =
        new(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => (IReadOnlyCollection<string>)(JsonSerializer.Deserialize<string[]>(v, JsonOptions) ?? Array.Empty<string>()));

    private static readonly ValueComparer<IReadOnlyCollection<string>> StringCollectionComparer =
        new(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => (IReadOnlyCollection<string>)c.ToList());

    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<Muscle> Muscles => Set<Muscle>();

    public DbSet<MeasurementType> MeasurementTypes => Set<MeasurementType>();

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<ExercisePrimaryMuscle> ExercisePrimaryMuscles => Set<ExercisePrimaryMuscle>();

    public DbSet<ExerciseSecondaryMuscle> ExerciseSecondaryMuscles => Set<ExerciseSecondaryMuscle>();

    public DbSet<ExerciseMeasurementType> ExerciseMeasurementTypes => Set<ExerciseMeasurementType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureMuscle(modelBuilder);
        ConfigureMeasurementType(modelBuilder);
        ConfigureExercise(modelBuilder);
        ConfigureExercisePrimaryMuscle(modelBuilder);
        ConfigureExerciseSecondaryMuscle(modelBuilder);
        ConfigureExerciseMeasurementType(modelBuilder);
    }

    private static void ConfigureMuscle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Muscle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Active).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.MuscleGroupIds)
                  .HasConversion(StringCollectionConverter)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired()
                  .Metadata.SetValueComparer(StringCollectionComparer);

            entity.HasIndex(e => e.Code)
                  .HasFilter("[IsDeleted] = 0")
                  .IsUnique();
        });
    }

    private static void ConfigureMeasurementType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeasurementType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Active).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.Code).HasDatabaseName("IX_MeasurementTypes_Code");
        });
    }

    private static void ConfigureExercise(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Difficulty).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Active).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Ignore(e => e.PrimaryMuscleIds);
            entity.Ignore(e => e.SecondaryMuscleIds);
            entity.Ignore(e => e.MeasurementTypeIds);

            entity.HasIndex(e => e.Code)
                  .HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL")
                  .IsUnique();
        });
    }

    private static void ConfigureExercisePrimaryMuscle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExercisePrimaryMuscle>(entity =>
        {
            entity.ToTable("ExercisePrimaryMuscles");
            entity.HasKey(e => new { e.ExerciseId, e.MuscleId });

            entity.Property(e => e.ExerciseId).HasMaxLength(32).IsRequired();
            entity.Property(e => e.MuscleId).HasColumnName("MuscleRef").HasMaxLength(100).IsRequired();
            entity.Property(e => e.SortOrder).IsRequired();

            entity.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Muscle>()
                .WithMany()
                .HasForeignKey(e => e.MuscleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.MuscleId);
        });
    }

    private static void ConfigureExerciseSecondaryMuscle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciseSecondaryMuscle>(entity =>
        {
            entity.ToTable("ExerciseSecondaryMuscles");
            entity.HasKey(e => new { e.ExerciseId, e.MuscleId });

            entity.Property(e => e.ExerciseId).HasMaxLength(32).IsRequired();
            entity.Property(e => e.MuscleId).HasColumnName("MuscleRef").HasMaxLength(100).IsRequired();
            entity.Property(e => e.SortOrder).IsRequired();

            entity.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Muscle>()
                .WithMany()
                .HasForeignKey(e => e.MuscleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.MuscleId);
        });
    }

    private static void ConfigureExerciseMeasurementType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciseMeasurementType>(entity =>
        {
            entity.ToTable("ExerciseMeasurementTypes");
            entity.HasKey(e => new { e.ExerciseId, e.MeasurementTypeId });

            entity.Property(e => e.ExerciseId).HasMaxLength(32).IsRequired();
            entity.Property(e => e.MeasurementTypeId).HasMaxLength(32).IsRequired();
            entity.Property(e => e.SortOrder).IsRequired();

            entity.HasOne<Exercise>()
                .WithMany()
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<MeasurementType>()
                .WithMany()
                .HasForeignKey(e => e.MeasurementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.MeasurementTypeId);
        });
    }
}
