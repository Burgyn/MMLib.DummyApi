using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom;
using MMLib.DummyApi.Features.Performance;
using MMLib.DummyApi.Features.System;
using MMLib.DummyApi.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<DummyApiOptions>(
    builder.Configuration.GetSection(DummyApiOptions.SectionName));

// Add services to the container
builder.Services.AddOpenApi();

// Authentication
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", options => { });

builder.Services.AddAuthorization();

// Infrastructure
builder.Services.AddSingleton<BackgroundJobService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BackgroundJobService>());

// Features
builder.Services.AddSystem();
builder.Services.AddPerformance();
builder.Services.AddCustomCollections();

var app = builder.Build();

// Load collections at startup and map dynamic endpoints
var dataStore = app.Services.GetRequiredService<CustomDataStore>();
var seeder = app.Services.GetRequiredService<AutoBogusSeeder>();
var options = app.Services.GetRequiredService<IOptions<DummyApiOptions>>().Value;
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Load collections from file
var filePath = options.CollectionsFile;
if (string.IsNullOrEmpty(filePath))
{
    filePath = Path.Combine(AppContext.BaseDirectory, "collections.json");
}

if (File.Exists(filePath))
{
    logger.LogInformation("Loading collections from {FilePath}", filePath);
    var json = File.ReadAllText(filePath);
    var collectionsFile = System.Text.Json.JsonSerializer.Deserialize<MMLib.DummyApi.Features.Custom.Models.CollectionsFile>(json, 
        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    
    if (collectionsFile?.Collections != null)
    {
        foreach (var definition in collectionsFile.Collections)
        {
            logger.LogInformation("Loading collection '{Name}' with seedCount={SeedCount}", 
                definition.Name, definition.SeedCount);
            
            // Save definition
            dataStore.SaveDefinition(definition);
            
            // Seed data if requested
            if (definition.SeedCount > 0)
            {
                var items = seeder.Generate(definition.Schema, definition.SeedCount);
                foreach (var item in items)
                {
                    dataStore.Add(definition.Name, item);
                }
                logger.LogInformation("Seeded {Count} items for collection '{Name}'", items.Count, definition.Name);
            }
            
            // Map dynamic endpoints for this collection
            app.MapCollectionEndpoints(definition);
        }
    }
}
else
{
    logger.LogInformation("No collections file found at {FilePath}", filePath);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseSimulation();
app.UseAuthentication();
app.UseAuthorization();

// Map static endpoints
app.MapSystem();
app.MapPerformance();
app.MapCustomCollections();

app.Run();
