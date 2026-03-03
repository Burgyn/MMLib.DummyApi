---
name: csharp-coding-conventions
description: C# coding style conventions and formatting rules. Use when writing, reviewing, or formatting C# code to ensure consistent indentation, naming, braces, line length, variable declarations, expression-bodied members, line wrapping, documentation, and code organization. Applies to all C# code including classes, methods, properties, constructors, namespaces, and file structure.
---

# C# Coding Conventions

## Overview

This skill defines C# coding style conventions and formatting rules. Follow these guidelines to ensure consistent code formatting, naming, and organization across all C# projects.

## Formatting Rules

### Empty Lines

Maximum of one consecutive empty line. Remove any extra blank lines.

### Trailing Spaces

Remove all trailing whitespace from lines.

### Braces

Curly braces MUST be written on a separate line.

Single-line statements MAY omit braces IF:
- The statement is on a separate line
- The statement is properly indented

Exception: Property accessors may use inline braces:
```csharp
public string Name { get { return _name; } }
```

### Line Length

Maximum line length: 130 characters.

Lines exceeding 130 characters MUST be wrapped across multiple lines.

When method signatures exceed the 130 character limit:
- Option A: Keep all parameters on one line (if it fits)
- Option B: Wrap ALL parameters to separate lines (including the first parameter)
- Do NOT wrap only some parameters

Exception: Unit test files MAY have longer lines if wrapping reduces readability.

## Naming Conventions

### Private/Internal Members

Use `_camelCase` with underscore prefix. Use `readonly` when possible.

```csharp
private readonly IService _service;
private int _count;
```

### Public Fields

Use `PascalCase` with no prefix. Avoid public fields whenever possible.

```csharp
public const int MaxSize = 100;
```

### Constants

Use `PascalCase` for constants.

```csharp
public const string DefaultName = "Unknown";
```

## Code Style

### Visibility

ALWAYS specify visibility explicitly, even if default.

```csharp
public class Example { }
internal class InternalExample { }
private class PrivateExample { }
```

### Namespaces

ALWAYS use file-scoped namespace declarations.

```csharp
namespace MyProject.Features;

public class Example { }
```

### Constructors

ALWAYS use primary constructors over traditional constructors.

```csharp
public class ExampleController(IService service) : ControllerBase
{
    private readonly IService _service = service;
}
```

### Type Names

Use language keywords, not BCL types.

```csharp
int count = 0;
string name = "";
object data = null;
```

Prefer:
- `int` over `Int32`
- `string` over `String`
- `object` over `Object`

### Member Access

Avoid `this.` unless absolutely necessary.

```csharp
private string _name;

public void SetName(string name)
{
    _name = name;
}
```

### Expression-Bodied Members

Use `=>` for single-expression methods and properties.

```csharp
public int CalculateTotal(int a, int b) => a + b;

public string FullName => $"{FirstName} {LastName}";
```

### String Literals

Use `nameof(...)` instead of string literals when relevant.

```csharp
throw new ArgumentNullException(nameof(parameter));
```

### Variable Declaration

Do NOT use `var` for variable declarations. Use explicit types.

Use target-typed `new()` expressions wherever the type is explicit: variable declarations, return statements, parameters, etc.

Prefer collection expressions syntax: `List<int> list = [1, 2, 3];` instead of `new List<int> { 1, 2, 3 }`.

```csharp
List<int> numbers = [1, 2, 3];
Dictionary<string, int> map = new();
string[] items = ["a", "b", "c"];
```

## Organization

### Imports

Place imports at file start, sorted alphabetically as single group (no System-first separation).

```csharp
using Microsoft.AspNetCore.Mvc;
using MyProject.Features;
using System.Collections.Generic;
using System.Linq;
```

### Members

All private member definitions MUST be placed at the beginning of the class.

```csharp
public class Example
{
    private readonly IService _service;
    private int _count;

    public Example(IService service)
    {
        _service = service;
    }

    public void DoSomething() { }
}
```

## Line Wrapping

Operators MUST be placed at the beginning of the continuation line, NOT at the end of the previous line.

Operators affected:
- `.` (member access)
- `=>` (lambda/expression body)
- `?:` (ternary operator)
- `:` (base constructor call)

Exception: Lambda with multi-line block body — place `=>` at end.

### Examples

**Constructor chaining:**
```csharp
public Foo(string p1, string p2)
    : base(p1, p2)
{
}
```

**Expression body:**
```csharp
private override void LongMethodName()
    => base.LongMethodName();
```

**Method chaining:**
```csharp
services.AddIdentityServer()
    .AddCredential()
    .AddClients(clients);
```

**Lambda exception - arrow at end:**
```csharp
builder.ConfigureServices(services =>
{
    services.AddSingleton<IEmailSender, Sender>();
});
```

## Documentation

### Inline Comments

Do NOT use `//` or `/* */` comments anywhere in code.

Code must be self-explanatory through clear naming and structure.

ONLY exception: XML documentation comments (`///`) for public members.

### Public Members

XML documentation comments are required for public members.

Include short `<summary>` and `<param name="parameterName">` comments.

Use `<inheritdoc/>` in implementation wherever possible.

```csharp
/// <summary>
/// Creates a new user account.
/// </summary>
/// <param name="email">The user's email address.</param>
/// <param name="name">The user's full name.</param>
/// <returns>The created user.</returns>
public User CreateUser(string email, string name)
{
}
```

```csharp
/// <inheritdoc/>
public override void Process()
{
}
```
