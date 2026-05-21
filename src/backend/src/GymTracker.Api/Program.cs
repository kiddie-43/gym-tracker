using GymTracker.Api.Options;
using GymTracker.Api.Middleware;
using GymTracker.Api.Swagger;
using GymTracker.Application.Admin.ExerciseFormTypes;
using GymTracker.Application.Admin.Exercises;
using GymTracker.Application.Admin.ExerciseTypes;
using GymTracker.Application.Admin.Common;
using GymTracker.Application.Admin.MeasurementTypes;
using GymTracker.Application.Admin.MuscleGroups;
using GymTracker.Application.Admin.Muscles;
using GymTracker.Application.Catalog;
using GymTracker.Application.Common;
using GymTracker.Application.Diets;
using GymTracker.Application.Meals;
using GymTracker.Application.Progress;
using GymTracker.Application.Features.Routines;
using GymTracker.Application.Features.Training;
using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Application.Routines;
using GymTracker.Application.Settings;
using GymTracker.Application.Workouts;
using GymTracker.Infrastructure.Caching;
using GymTracker.Infrastructure.Firebase;
using GymTracker.Infrastructure.Firebase.Repositories;
using GymTracker.Infrastructure.Observability;
using GymTracker.Infrastructure.Sql;
using GymTracker.Infrastructure.Storage;
using GymTracker.Infrastructure.Wger;
using GymTracker.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
	builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection(FirebaseOptions.SectionName));
builder.Services.Configure<WgerOptions>(builder.Configuration.GetSection(WgerOptions.SectionName));
builder.Services.Configure<SqlOptions>(builder.Configuration.GetSection(SqlOptions.SectionName));

var sqlOptions = builder.Configuration.GetSection(SqlOptions.SectionName).Get<SqlOptions>();

builder.Services.AddControllers();
builder.Services
	.AddAuthentication(AuthSchemes.FirebaseBearer)
	.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, PassthroughAuthenticationHandler>(
		AuthSchemes.FirebaseBearer,
		_ => { });
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy(AuthorizationPolicies.DefaultUserPolicy, policy =>
	{
		policy.RequireAuthenticatedUser();
	});

	options.AddPolicy(AuthorizationPolicies.AdminOnlyPolicy, policy =>
	{
		policy.RequireAuthenticatedUser();
		policy.RequireClaim("custom.admin", "true");
	});
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddPolicy("FrontendDev", policy =>
	{
		policy
			.WithOrigins(
				"http://localhost:5173",
				"http://localhost:5174",
				"http://localhost:5175",
				"http://localhost:5176",
				"http://localhost:5177"
			)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});
builder.Services.AddSingleton<CorrelationIdAccessor>();
if (sqlOptions is { Enabled: true } && !string.IsNullOrWhiteSpace(sqlOptions.ConnectionString))
{
	builder.Services.AddSingleton(new SqlDocumentStore(sqlOptions.ConnectionString!));
}
else
{
	builder.Services.AddSingleton<FirestoreContext>();
	builder.Services.AddSingleton<IAdminDocumentStore, FirestoreAdminDocumentStore>();
}
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<IExerciseStorageService>(provider => provider.GetRequiredService<StorageService>());
builder.Services.AddSingleton<CatalogCacheRepository>();
builder.Services.AddSingleton<CatalogCacheMetadataStore>();
builder.Services.AddSingleton<ICatalogAvailabilityStore>(provider => provider.GetRequiredService<CatalogCacheMetadataStore>());
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<WgerCatalogMapper>();
builder.Services.AddHttpClient<WgerApiClient>();
builder.Services.AddScoped<ICatalogDataSource, CatalogCacheService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddSingleton<ProgressCalculationService>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<IProgressSnapshotRepository, ProgressSnapshotRepository>();
builder.Services.AddScoped<GymTracker.Application.Interfaces.Persistence.IRoutineRepository, SqlRoutineAggregateRepository>();
builder.Services.AddScoped<GymTracker.Application.Routines.IRoutineRepository, GymTracker.Infrastructure.Firebase.RoutineRepository>();
builder.Services.AddScoped<ITrainingFlowRepository, SqlTrainingFlowRepository>();
builder.Services.AddScoped<IDietRepository, DietRepository>();
builder.Services.AddScoped<IMealLogRepository, MealLogRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddScoped<WorkoutService>();
builder.Services.AddScoped<WorkoutCatalogService>();
builder.Services.AddScoped<ProgressService>();
builder.Services.AddScoped<RoutineService>();
builder.Services.AddScoped<RoutinesCommandHandlers>();
builder.Services.AddScoped<RoutineSessionsCommandHandlers>();
builder.Services.AddScoped<SessionExercisesHandlers>();
builder.Services.AddScoped<PlannedSetsHandlers>();
builder.Services.AddScoped<ExerciseCatalogFallbackService>();
builder.Services.AddScoped<ProgressComparisonService>();
builder.Services.AddScoped<ExerciseTrainingLogHandlers>();
builder.Services.AddScoped<TrainingFlowHandlers>();
builder.Services.AddScoped<DietService>();
builder.Services.AddScoped<MealLogService>();
builder.Services.AddScoped<UserPreferencesService>();
builder.Services.AddScoped<WorkoutHistoryService>();
builder.Services.AddScoped<MealHistoryService>();
builder.Services.AddScoped<CatalogAvailabilityService>();
builder.Services.AddSingleton<IMuscleGroupRepository, MuscleGroupRepository>();
builder.Services.AddSingleton<MuscleRepository>();
if (sqlOptions is { Enabled: true } && !string.IsNullOrWhiteSpace(sqlOptions.ConnectionString))
{
	builder.Services.AddScoped(_ => new SqlMuscleRepository(sqlOptions.ConnectionString!));
	builder.Services.AddScoped<IMuscleRepository, ResilientSqlMuscleRepository>();
}
else
{
	builder.Services.AddSingleton<IMuscleRepository>(provider => provider.GetRequiredService<MuscleRepository>());
}
builder.Services.AddSingleton<IExerciseTypeRepository, ExerciseTypeRepository>();
builder.Services.AddSingleton<IExerciseFormTypeRepository, ExerciseFormTypeRepository>();
builder.Services.AddSingleton<ExerciseRepository>();
if (sqlOptions is { Enabled: true } && !string.IsNullOrWhiteSpace(sqlOptions.ConnectionString))
{
	builder.Services.AddScoped(_ => new SqlExerciseRepository(sqlOptions.ConnectionString!));
	builder.Services.AddScoped<IExerciseRepository, ResilientSqlExerciseRepository>();
}
else
{
	builder.Services.AddSingleton<IExerciseRepository>(provider => provider.GetRequiredService<ExerciseRepository>());
}
builder.Services.AddScoped<MuscleGroupService>();
builder.Services.AddScoped<MuscleCsvImportService>();
builder.Services.AddScoped<MuscleService>();
builder.Services.AddScoped<ExerciseTypeService>();
builder.Services.AddScoped<ExerciseFormTypeService>();

if (sqlOptions is { Enabled: true } && !string.IsNullOrWhiteSpace(sqlOptions.ConnectionString))
{
	builder.Services.AddSingleton<InMemoryMeasurementTypeRepository>();
	builder.Services.AddScoped(_ => new SqlMeasurementTypeRepository(sqlOptions.ConnectionString!));
	builder.Services.AddScoped<IMeasurementTypeRepository, ResilientSqlMeasurementTypeRepository>();
}
else
{
	builder.Services.AddSingleton<InMemoryMeasurementTypeRepository>();
	builder.Services.AddScoped<MeasurementTypeRepository>();
	builder.Services.AddScoped<IMeasurementTypeRepository, ResilientMeasurementTypeRepository>();
}

builder.Services.AddScoped<MeasurementTypeService>();
builder.Services.AddScoped<IExerciseRelationsValidator, ExerciseRelationsValidator>();
builder.Services.AddScoped<ExerciseMediaCompensationService>();
builder.Services.AddScoped<ExerciseService>();
builder.Services.AddScoped<DashboardSummaryRepository>();
builder.Services.AddTransient<Microsoft.Extensions.Options.IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>, OpenApiConfiguration>();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("FrontendDev");
if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseMiddleware<FirebaseAuthMiddleware>();
app.UseMiddleware<CurrentUserContextMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program;
