using Microsoft.AspNetCore.Authentication;
using MMLib.DummyApi.Configuration;
using MMLib.DummyApi.Features.Custom;
using MMLib.DummyApi.Features.Performance;
using MMLib.DummyApi.Features.System;
using MMLib.DummyApi.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DummyApiOptions>(
    builder.Configuration.GetSection(DummyApiOptions.SectionName));

builder.Services.AddOpenApi(options =>
{
    options.AddCollectionOpenApiTransformers();
});

builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", options => { });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<BackgroundJobService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BackgroundJobService>());

builder.Services.AddSystem();
builder.Services.AddPerformance();
builder.Services.AddCustomCollections();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseSimulation();
app.UseAuthentication();
app.UseAuthorization();

app.MapSystem();
app.MapPerformance();
app.MapCustomCollections();

app.LoadAndMapCollections();

app.Run();

public partial class Program { }
