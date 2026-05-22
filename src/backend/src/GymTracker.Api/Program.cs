using GymTracker.Api.Options;
using GymTracker.Api.Middleware;
using GymTracker.Api.Swagger;
using GymTracker.Application.Admin.Exercises;
using GymTracker.Application.Admin.MeasurementTypes;
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
using GymTracker.Infrastructure.Admin;
using GymTracker.Infrastructure.Caching;
using GymTracker.Infrastructure.Exercises;
using GymTracker.Infrastructure.Firebase; // CatalogCacheRepository, InMemoryMeasurementTypeRepository, DashboardSummaryRepository (no usan Firebase)
using GymTracker.Infrastructure.Observability;
using GymTracker.Infrastructure.Sql;
using GymTracker.Infrastructure.Wger;
using Microsoft.EntityFrameworkCore;
using GymTracker.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
	builder.Configuration.AddUserSecrets<Program>(optional: true);
}

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
if (!string.IsNullOrWhiteSpace(sqlOptions?.ConnectionString))
{
	builder.Services.AddSingleton(new SqlDocumentStore(sqlOptions.ConnectionString!));
}
builder.Services.AddSingleton<CatalogCacheRepository>();
builder.Services.AddSingleton<CatalogCacheMetadataStore>();
builder.Services.AddSingleton<ICatalogAvailabilityStore>(provider => provider.GetRequiredService<CatalogCacheMetadataStore>());
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<WgerCatalogMapper>();
builder.Services.AddHttpClient<WgerApiClient>();
builder.Services.AddScoped<ICatalogDataSource, CatalogCacheService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddSingleton<ProgressCalculationService>();
builder.Services.AddScoped<IWorkoutRepository, SqlWorkoutRepository>();
builder.Services.AddScoped<IProgressSnapshotRepository, SqlProgressSnapshotRepository>();
builder.Services.AddScoped<GymTracker.Application.Interfaces.Persistence.IRoutineRepository, SqlRoutineAggregateRepository>();
builder.Services.AddScoped<GymTracker.Application.Routines.IRoutineRepository, SqlLegacyRoutineRepository>();
builder.Services.AddScoped<ITrainingFlowRepository, SqlTrainingFlowRepository>();
builder.Services.AddScoped<IDietRepository, SqlDietRepository>();
builder.Services.AddScoped<IMealLogRepository, SqlMealLogRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, SqlUserPreferencesRepository>();
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
builder.Services.AddScoped<UserPreferencesService>();
builder.Services.AddScoped<WorkoutHistoryService>();
builder.Services.AddScoped<CatalogAvailabilityService>();


if (!string.IsNullOrWhiteSpace(sqlOptions?.ConnectionString))
{
	builder.Services.AddDbContext<AdminDbContext>(options =>
		options.UseSqlServer(sqlOptions.ConnectionString));
	builder.Services.AddScoped<IMuscleRepository, GymTracker.Infrastructure.Muscles.MuscleRepository>();
	builder.Services.AddScoped<IExerciseRepository, SqlExerciseRepository>();
	builder.Services.AddScoped<IMeasurementTypeRepository, GymTracker.Infrastructure.MeasurementTypes.SqlMeasurementTypeRepository>();
}

builder.Services.AddScoped<ExerciseService>();
builder.Services.AddScoped<MuscleCsvImportService>();
builder.Services.AddScoped<MuscleService>();

builder.Services.AddScoped<MeasurementTypeService>();
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
