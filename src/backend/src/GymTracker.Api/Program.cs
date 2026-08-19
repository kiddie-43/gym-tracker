using GymTracker.Api.Extensions;
using GymTracker.Application;
using GymTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfiguration();

builder.Services.AddApiServices(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecksConfiguration();

var app = builder.Build();

app.ApplyDatabaseMigrations();
app.UseApiPipeline();

app.Run();

public partial class Program;
