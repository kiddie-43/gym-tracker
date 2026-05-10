using GymTracker.Api.Options;
using GymTracker.Api.Middleware;
using GymTracker.Api.Swagger;
using GymTracker.Application.Catalog;
using GymTracker.Application.Diets;
using GymTracker.Application.Meals;
using GymTracker.Application.Progress;
using GymTracker.Application.Routines;
using GymTracker.Application.Settings;
using GymTracker.Application.Workouts;
using GymTracker.Infrastructure.Caching;
using GymTracker.Infrastructure.Firebase;
using GymTracker.Infrastructure.Observability;
using GymTracker.Infrastructure.Wger;
using GymTracker.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
	builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection(FirebaseOptions.SectionName));
builder.Services.Configure<WgerOptions>(builder.Configuration.GetSection(WgerOptions.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddPolicy("FrontendDev", policy =>
	{
		policy
			.WithOrigins("http://localhost:5173", "http://localhost:5174")
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});
builder.Services.AddSingleton<CorrelationIdAccessor>();
builder.Services.AddSingleton<FirestoreContext>();
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
builder.Services.AddScoped<IRoutineRepository, RoutineRepository>();
builder.Services.AddScoped<IDietRepository, DietRepository>();
builder.Services.AddScoped<IMealLogRepository, MealLogRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddScoped<WorkoutService>();
builder.Services.AddScoped<ProgressService>();
builder.Services.AddScoped<RoutineService>();
builder.Services.AddScoped<DietService>();
builder.Services.AddScoped<MealLogService>();
builder.Services.AddScoped<UserPreferencesService>();
builder.Services.AddScoped<WorkoutHistoryService>();
builder.Services.AddScoped<MealHistoryService>();
builder.Services.AddScoped<CatalogAvailabilityService>();
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
app.UseHttpsRedirection();
app.UseMiddleware<FirebaseAuthMiddleware>();
app.UseMiddleware<CurrentUserContextMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program;
