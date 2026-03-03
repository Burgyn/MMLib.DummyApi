---
name: dotnet-technology-stack
description: Technology stack, conventions, and best practices for .NET/C# projects. Use when working with .NET 10+ projects, C# code, adding NuGet packages, selecting databases, setting up testing, or following project-specific conventions.
---

# .NET Technology Stack

## Overview

This skill defines the technology stack, conventions, and best practices for .NET/C# projects. Follow these guidelines to ensure consistency across all .NET development work.

## Technology Stack

- **.NET Version**: Always use .NET 10 or later
- **C# Language**: Use the latest C# version with its newest features
- **Language Features**: Leverage modern C# capabilities including:
  - Primary constructors
  - Collection expressions
  - LINQ improvements
  - Pattern matching enhancements
  - File-scoped namespaces
  - Required members
  - And other latest C# language features

## Package Management

**Always use the `dotnet add package` command** to add NuGet packages. Never manually edit `.csproj` files to add package references.

**Command format:**
```bash
dotnet add package <package-name>
```

**Examples:**
```bash
dotnet add package Scalar.AspNetCore
dotnet add package LiteDB
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package xunit
dotnet add package NSubstitute
```

## OpenAPI Documentation

Use **Scalar** for displaying OpenAPI/Swagger documentation in ASP.NET Core applications.

**Package:**
```bash
dotnet add package Scalar.AspNetCore
```

**Integration Example:**

Add the using directive and configure Scalar in `Program.cs`:

```csharp
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
```

**Access:** The Scalar API Reference is accessible at `/scalar` by default.

**Customization:** You can customize the route and options:

```csharp
app.MapScalarApiReference("/docs", options =>
{
    options.WithTitle("My API");
});
```

## Database Selection

Choose the database technology based on project type:

### Demo Projects

For demo, proof-of-concept, or sample projects, use **LiteDB** with an in-memory database.

**Package:**
```bash
dotnet add package LiteDB
```

**Pattern:**
```csharp
_db = new LiteDatabase("Filename=:memory:");
```

This creates an in-memory database that doesn't persist data between application restarts, perfect for demos and testing.

### Real Projects

For production or real applications, use **Entity Framework Core**.

**Packages:**
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  # For SQL Server
# Or other provider-specific packages as needed
```

Use Entity Framework Core with SQL Server database providers.

## Data Generation

Use **AutoBogus** (or **Bogus**) for generating dummy/test data.

**Package:**
```bash
dotnet add package AutoBogus
```

**Usage:** AutoBogus provides automatic fake data generation for testing and development purposes.

## Restricted Packages

**Do NOT use the following packages** as they are paid/commercial:

- **FluentAssertion** - Use XUnit's built-in assertions or standard .NET assertions instead
- **MediatR** - Use alternative patterns or libraries that are free/open-source

When alternatives are needed, choose free and open-source packages that provide similar functionality.

## Testing Framework

### Unit Testing

Use **XUnit** as the unit testing framework.

**Packages:**
```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
```

### Mocking

Use **NSubstitute** for creating mocks and test doubles.

**Package:**
```bash
dotnet add package NSubstitute
```

**Example:**
```csharp
using NSubstitute;
using Xunit;

public class UserServiceShould
{
    [Fact]
    public void ReturnNullWhenUserNotFound()
    {
        var repository = Substitute.For<IUserRepository>();
        repository.GetById(1).Returns((User)null);
        
        var service = new UserService(repository);
        var result = service.GetUser(1);
        
        Assert.Null(result);
    }
}
```

## Naming Conventions

### Test Class Names

Test class names **must end with the `Should` suffix**.

**Examples:**
- `UserServiceShould`
- `OrderProcessorShould`
- `PaymentGatewayShould`
- `EmailValidatorShould`

### Test Method Names

Test method names continue the sentence after "should" - they complete the thought started by the class name.

**Pattern:** `ClassNameShould.MethodNameContinuesAfterShould`

**Examples:**
- `UserServiceShould.ReturnNullWhenUserNotFound`
- `UserServiceShould.ThrowExceptionWhenEmailIsInvalid`
- `OrderProcessorShould.ProcessOrderSuccessfully`
- `OrderProcessorShould.RejectOrderWhenInventoryInsufficient`
- `PaymentGatewayShould.ProcessPaymentWhenValid`
- `PaymentGatewayShould.DeclinePaymentWhenInsufficientFunds`

**Guidelines:**
- Method names should read as a complete sentence when combined with the class name
- Use descriptive action verbs (Return, Throw, Process, Reject, etc.)
- Include the condition or scenario (WhenUserNotFound, WhenEmailIsInvalid, etc.)

## Architecture

Architecture patterns, design principles, and structural guidelines are documented in a separate skill. This skill focuses solely on technology choices, package selection, and coding conventions.
