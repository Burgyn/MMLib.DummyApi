using System.Text.Json;
using Bogus;

namespace MMLib.DummyApi.Features.Custom;

/// <summary>
/// Generates fake data using Bogus based on JSON schema.
/// Uses smart field name mapping to generate realistic data.
/// </summary>
public class AutoBogusSeeder
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Generates fake data items based on JSON schema.
    /// </summary>
    /// <param name="schema">The JSON schema describing the item shape.</param>
    /// <param name="count">Number of items to generate.</param>
    public List<JsonElement> Generate(JsonElement? schema, int count)
    {
        if (count <= 0) return [];

        List<JsonElement> results = [];

        if (schema == null || schema.Value.ValueKind != JsonValueKind.Object)
        {
            for (int i = 0; i < count; i++)
            {
                var json = JsonSerializer.Serialize(new { name = $"Item {i + 1}" });
                results.Add(JsonDocument.Parse(json).RootElement.Clone());
            }
            return results;
        }

        for (int i = 0; i < count; i++)
        {
            var item = GenerateFromSchema(schema.Value);
            var json = JsonSerializer.Serialize(item, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            results.Add(JsonDocument.Parse(json).RootElement.Clone());
        }

        return results;
    }

    private Dictionary<string, object?> GenerateFromSchema(JsonElement schema)
    {
        Dictionary<string, object?> result = new();

        if (!schema.TryGetProperty("properties", out var properties))
            return result;

        foreach (var prop in properties.EnumerateObject())
        {
            var value = GenerateValue(prop.Name, prop.Value);
            result[prop.Name] = value;
        }

        return result;
    }

    private object? GenerateValue(string fieldName, JsonElement propertySchema)
    {
        var fieldNameLower = fieldName.ToLowerInvariant();

        var jsonType = "string";
        if (propertySchema.TryGetProperty("type", out var typeElement))
        {
            jsonType = typeElement.GetString() ?? "string";
        }

        if (propertySchema.TryGetProperty("enum", out var enumElement))
        {
            var values = enumElement.EnumerateArray().Select(e => e.GetString()).ToList();
            return _faker.PickRandom(values);
        }

        if (propertySchema.TryGetProperty("format", out var formatElement))
        {
            var format = formatElement.GetString();
            return format switch
            {
                "email" => _faker.Internet.Email(),
                "uri" or "url" => _faker.Internet.Url(),
                "uuid" => Guid.NewGuid().ToString(),
                "date" => _faker.Date.Past().ToString("yyyy-MM-dd"),
                "date-time" => _faker.Date.Past().ToString("o"),
                _ => GenerateByFieldName(fieldNameLower, jsonType)
            };
        }

        return GenerateByFieldName(fieldNameLower, jsonType);
    }

    private object? GenerateByFieldName(string fieldName, string jsonType)
        => fieldName switch
        {
            "firstname" or "first_name" or "givenname" => _faker.Name.FirstName(),
            "lastname" or "last_name" or "familyname" or "surname" => _faker.Name.LastName(),
            "fullname" or "full_name" or "displayname" => _faker.Name.FullName(),
            "name" when jsonType == "string" => _faker.Name.FullName(),
            "username" or "user_name" or "login" => _faker.Internet.UserName(),
            "email" or "emailaddress" or "email_address" => _faker.Internet.Email(),
            "phone" or "phonenumber" or "phone_number" or "mobile" or "telephone" => _faker.Phone.PhoneNumber(),
            "age" => _faker.Random.Int(18, 80),
            "gender" or "sex" => _faker.PickRandom(new[] { "Male", "Female", "Other" }),
            "birthdate" or "birth_date" or "dateofbirth" or "dob" => _faker.Date.Past(50, DateTime.Now.AddYears(-18)).ToString("yyyy-MM-dd"),
            "avatar" or "picture" or "photo" => _faker.Internet.Avatar(),

            "address" or "streetaddress" or "street_address" or "street" => _faker.Address.StreetAddress(),
            "city" => _faker.Address.City(),
            "state" or "province" or "region" => _faker.Address.State(),
            "country" => _faker.Address.Country(),
            "countrycode" or "country_code" => _faker.Address.CountryCode(),
            "zipcode" or "zip_code" or "postalcode" or "postal_code" or "zip" => _faker.Address.ZipCode(),
            "latitude" or "lat" => _faker.Address.Latitude(),
            "longitude" or "lng" or "lon" => _faker.Address.Longitude(),

            "company" or "companyname" or "company_name" or "organization" => _faker.Company.CompanyName(),
            "department" => _faker.Commerce.Department(),
            "jobtitle" or "job_title" or "position" or "title" when jsonType == "string" => _faker.Name.JobTitle(),

            "productname" or "product_name" => _faker.Commerce.ProductName(),
            "price" or "amount" or "cost" or "total" or "totalamount" or "total_amount" => _faker.Finance.Amount(1, 1000),
            "category" => _faker.Commerce.Categories(1).First(),
            "sku" or "productcode" or "product_code" => _faker.Commerce.Ean13(),
            "color" or "colour" => _faker.Commerce.Color(),
            "material" => _faker.Commerce.ProductMaterial(),
            "quantity" or "qty" or "count" or "stock" or "stockquantity" or "stock_quantity" => _faker.Random.Int(1, 100),

            "url" or "website" or "link" or "homepage" => _faker.Internet.Url(),
            "imageurl" or "image_url" or "image" or "thumbnail" => _faker.Image.PicsumUrl(),
            "ip" or "ipaddress" or "ip_address" => _faker.Internet.Ip(),
            "domain" or "domainname" => _faker.Internet.DomainName(),

            "description" or "bio" or "about" or "summary" => _faker.Lorem.Sentence(10),
            "content" or "body" or "text" => _faker.Lorem.Paragraph(),
            "comment" or "note" or "notes" => _faker.Lorem.Sentence(),
            "headline" or "tagline" or "slogan" => _faker.Company.CatchPhrase(),

            "id" or "customerid" or "customer_id" or "userid" or "user_id" or "orderid" or "order_id" => Guid.NewGuid().ToString(),

            "createdat" or "created_at" or "createddate" or "created_date" => _faker.Date.Past().ToString("o"),
            "updatedat" or "updated_at" or "modifiedat" or "modified_at" => _faker.Date.Recent().ToString("o"),
            "date" => _faker.Date.Past().ToString("yyyy-MM-dd"),

            "status" => _faker.PickRandom(new[] { "active", "inactive", "pending" }),
            "isactive" or "is_active" or "active" or "enabled" => _faker.Random.Bool(),
            "verified" or "isverified" or "is_verified" or "confirmed" => _faker.Random.Bool(),

            _ => jsonType switch
            {
                "string" => _faker.Lorem.Word(),
                "integer" => _faker.Random.Int(1, 100),
                "number" => _faker.Random.Decimal(1, 1000),
                "boolean" => _faker.Random.Bool(),
                "array" => new List<object>(),
                _ => _faker.Lorem.Word()
            }
        };
}
