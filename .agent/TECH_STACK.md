# MMLib.DummyApi – Technology Stack

## Runtime and Framework

| Component | Version |
| --------- | ------- |
| .NET | 10.0 |
| Target Framework | `net10.0` |
| ASP.NET Core | 10.0 |
| API Style | Minimal APIs |

## Language Features

- **C#** with nullable reference types (`Nullable` enabled)
- **Implicit usings** enabled (global `using` directives)

## NuGet Packages

### Main Application

| Package | Version | Purpose |
| ------- | ------- | ------- |
| AutoBogus | 2.13.1 | Fake data generation from JSON Schema |
| JsonSchema.Net | 8.0.5 | JSON Schema validation |
| LiteDB | 5.0.21 | In-memory document storage |
| Microsoft.AspNetCore.OpenApi | 10.0.0 | OpenAPI support |
| Microsoft.OpenApi | 2.0.0 | OpenAPI model |
| Scalar.AspNetCore | 2.12.6 | API documentation UI |

### Test Project

| Package | Version | Purpose |
| ------- | ------- | ------- |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.0 | Integration testing with WebApplicationFactory |
| Microsoft.NET.Test.Sdk | 17.13.0 | Test SDK |
| xunit | 2.9.3 | Test framework |
| xunit.runner.visualstudio | 3.0.2 | Test runner |

## Container

- **Image**: `burgyn/mmlib-dummyapi:latest`
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:10.0`
- **Port**: 8080

## Project Structure

```text
MMLib.DummyApi/
├── MMLib.DummyApi.sln
├── src/
│   └── MMLib.DummyApi/
│       ├── Program.cs
│       ├── MMLib.DummyApi.csproj
│       ├── Configuration/
│       ├── Features/
│       │   ├── Custom/
│       │   ├── Performance/
│       │   └── System/
│       └── Infrastructure/
└── tests/
    └── MMLib.DummyApi.Tests/
        └── MMLib.DummyApi.Tests.csproj
```

## Configuration

- **appsettings.json** – DummyApi section, logging, allowed hosts
- **collections.json** – Collection definitions (copied to output)
- **Environment variables** – `DUMMYAPI__*` prefix for configuration override
