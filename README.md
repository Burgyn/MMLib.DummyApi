# MMLib.DummyApi

A dynamic REST API for integration testing, benchmarking, and UI mocking. Define your own collections with JSON schemas, auto-generated fake data, and mockoon-style response rules.

## Features

- **Dynamic Collections**: Define collections with JSON schemas at startup or via API
- **AutoBogus Integration**: Automatically generate realistic fake data based on field names
- **Response Rules**: Mockoon-style template rules with conditions and custom responses
- **Simulation Headers**: Retry, delay, error, and chaos simulation on any endpoint
- **Background Jobs**: Simulate async processing with configurable field updates
- **Per-collection OpenAPI**: Each collection gets its own endpoints visible in Scalar
- **Docker Ready**: Mount your collections file and run anywhere

## Quick Start

### Run Locally

```bash
dotnet run --project src/MMLib.DummyApi/MMLib.DummyApi.csproj
```

### Docker

```bash
# Build
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer

# Run with default collections
docker run -p 8080:8080 burgyn/mmlib-dummyapi

# Run with custom collections file
docker run -p 8080:8080 \
  -v /path/to/my-collections.json:/config/collections.json \
  -e DUMMYAPI__COLLECTIONSFILE=/config/collections.json \
  burgyn/mmlib-dummyapi
```

## Configuration

### Collections File (collections.json)

```json
{
  "collections": [
    {
      "name": "products",
      "displayName": "Products",
      "description": "Product catalog",
      "authRequired": false,
      "seedCount": 50,
      "schema": {
        "type": "object",
        "required": ["name", "price"],
        "properties": {
          "name": { "type": "string" },
          "price": { "type": "number" },
          "category": { "type": "string" }
        }
      },
      "backgroundJob": {
        "fieldPath": "calculatedPrice",
        "operation": "sum:price",
        "delayMs": 2000
      },
      "rules": [
        {
          "priority": 1,
          "method": "GET",
          "when": [
            { "source": "query", "field": "error", "operator": "equals", "value": "true" }
          ],
          "response": {
            "statusCode": 500,
            "body": { "error": "Simulated error" }
          }
        }
      ]
    }
  ]
}
```

### Collection Definition Properties

| Property | Description |
|----------|-------------|
| `name` | URL path name (required) |
| `displayName` | Display name for OpenAPI |
| `description` | OpenAPI description |
| `authRequired` | Require X-Api-Key header |
| `seedCount` | Number of items to auto-generate |
| `schema` | JSON Schema for validation |
| `backgroundJob` | Background processing config |
| `rules` | Response rules (mockoon-style) |

### Environment Variables

- `DUMMYAPI__COLLECTIONSFILE` - Path to collections JSON file
- `DUMMYAPI__DEFAULTAPIKEY` - API key for authenticated collections (default: `test-api-key-123`)
- `DUMMYAPI__BACKGROUNDJOBDELAYMS` - Default background job delay

## API Endpoints

### Collection Management

- `GET /custom/_definitions` - List all collection definitions
- `GET /custom/_definitions/{name}` - Get collection definition
- `POST /custom/_definitions` - Create new collection
- `PUT /custom/_definitions/{name}` - Update collection definition
- `DELETE /custom/_definitions/{name}` - Delete collection

### Dynamic Collection Endpoints

For each collection (e.g., `products`):

- `GET /products` - List all items
- `GET /products/{id}` - Get item by ID
- `POST /products` - Create item
- `PUT /products/{id}` - Update item
- `DELETE /products/{id}` - Delete item

### System

- `POST /reset` - Reset all collections
- `POST /reset?collection=products` - Reset specific collection
- `GET /health` - Health check

### Performance

- `GET /perf/payload?size=1mb` - Generate payload
- `GET /perf/counter` - Get counter value
- `POST /perf/counter/increment` - Increment counter

## AutoBogus Smart Field Mapping

AutoBogus automatically generates realistic data based on property names:

| Field Name | Generated Data |
|------------|----------------|
| `firstName`, `first_name` | Realistic first name |
| `lastName`, `last_name` | Realistic last name |
| `email` | Valid email address |
| `phone`, `phoneNumber` | Phone number |
| `address`, `streetAddress` | Street address |
| `city` | City name |
| `country` | Country name |
| `price`, `amount` | Decimal amount |
| `company`, `companyName` | Company name |
| `description` | Lorem ipsum text |
| ... | And many more |

## Response Rules

Define mockoon-style rules for custom responses:

```json
{
  "rules": [
    {
      "priority": 1,
      "method": "GET",
      "when": [
        { "source": "query", "field": "status", "operator": "equals", "value": "vip" }
      ],
      "response": {
        "statusCode": 200,
        "body": { "message": "VIP response" },
        "headers": { "X-VIP": "true" },
        "delayMs": 500
      }
    }
  ]
}
```

### Condition Sources

- `query` - Query string parameters
- `header` - Request headers
- `body` - Request body JSON path
- `path` - Route parameters

### Condition Operators

- `equals`, `contains`, `startsWith`, `endsWith`
- `greaterThan`, `lessThan`, `range`
- `exists`, `notExists`

## Simulation Headers

Apply to any endpoint:

| Header | Description |
|--------|-------------|
| `X-Simulate-Delay: 500` | Add 500ms delay |
| `X-Simulate-Error: true` | Return 500 error |
| `X-Simulate-Retry: 3` | Fail first N-1 requests |
| `X-Request-Id: unique-id` | Track retry requests |
| `X-Chaos-FailureRate: 0.3` | 30% chance of 500 |
| `X-Chaos-LatencyRange: 100-500` | Random delay in range |
| `X-Background-Delay: 5000` | Override background job delay |

## Background Job Operations

Configure automatic field updates after creation:

| Operation | Example | Description |
|-----------|---------|-------------|
| `sequence` | `sequence:pending,processing,completed` | Cycle through values |
| `sum` | `sum:items.price` | Sum array field values |
| `count` | `count:items` | Count array items |
| `timestamp` | `timestamp` | Set current UTC time |
| `random` | `random:1,100` | Random number in range |

## Examples

### Create Collection via API

```bash
curl -X POST http://localhost:5000/custom/_definitions \
  -H "Content-Type: application/json" \
  -d '{
    "name": "users",
    "displayName": "Users",
    "seedCount": 10,
    "schema": {
      "type": "object",
      "properties": {
        "firstName": { "type": "string" },
        "lastName": { "type": "string" },
        "email": { "type": "string" }
      }
    }
  }'
```

### Test Retry Logic

```bash
for i in {1..3}; do
  curl -X GET http://localhost:5000/products \
    -H "X-Simulate-Retry: 3" \
    -H "X-Request-Id: test-123"
done
```

### Use Response Rules

```bash
# Returns custom VIP response
curl "http://localhost:5000/customers?status=vip"

# Returns simulated error
curl "http://localhost:5000/products?category=error"
```

## OpenAPI Documentation

- Scalar UI: `http://localhost:5000/scalar/v1`
- OpenAPI JSON: `http://localhost:5000/openapi/v1.json`

## License

See LICENSE file for details.

## ToDo:

- [] celkova dokumentacia
- [] refaktor jednotlivych tried, je to bordel
- [] filter nefunguje
- testy na rules a bacground job
  - dynamicky vytvorit fixture taky ze tam bude specificka kolekcia
  - budu tam priklady na vsetky moze scenare rules aj backrground
  - pozor na trvanie
