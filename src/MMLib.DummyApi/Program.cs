using System.Text.Json;
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
builder.Services.AddOpenApi(options =>
{
    options.AddCollectionOpenApiTransformers();
});

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

// Load collections from file and map dynamic endpoints
app.LoadAndMapCollections();

app.Run();
