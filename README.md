# MMLib.DummyApi

A dummy REST API for integration testing demonstrations and benchmark tools. This API provides realistic scenarios including CRUD operations, retry simulation, background jobs, and performance testing endpoints.

## Features

- **CRUD Operations**: Products (public) and Orders (authenticated) domains
- **Custom Collections**: Define your own dynamic endpoints with any JSON structure
- **Retry Simulation**: Simulate retry scenarios via headers
- **Error Simulation**: Test error handling with configurable failures
- **Background Jobs**: Simulate async processing with delayed updates
- **Performance Testing**: Payload generation and counter endpoints
- **Authorization**: Simple API key authentication for Orders endpoints
- **JSON Schema Validation**: Optional validation for custom collections

## Quick Start

### Run Locally

```bash
dotnet run --project src/MMLib.DummyApi/MMLib.DummyApi.csproj
```

### Docker

```bash
# Build container image
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer

# Or use Docker directly
docker build -t mmlib-dummyapi .
docker run -p 8080:8080 mmlib-dummyapi
```

## Configuration

Configuration is done via `appsettings.json` or environment variables:

```json
{
  "DummyApi": {
    "InitialProductCount": 50,
    "InitialOrderCount": 20,
    "DefaultApiKey": "test-api-key-123",
    "BackgroundJobDelayMs": 2000,
    "Performance": {
      "MaxPayloadSizeMb": 10,
      "MaxDelayMs": 30000
    }
  }
}
```

Environment variables use double underscore notation:
- `DUMMYAPI__INITIALPRODUCTCOUNT=100`
- `DUMMYAPI__DEFAULTAPIKEY=my-key`

## API Endpoints

### Products (No Authentication)

- `GET /products` - List all products (supports `?category=Electronics&minPrice=100`)
- `GET /products/{id}` - Get product by ID
- `POST /products` - Create a new product
- `PUT /products/{id}` - Update product
- `DELETE /products/{id}` - Delete product
- `GET /products/{id}/status` - Get background job status

### Orders (Requires Authentication)

All endpoints require `X-Api-Key` header.

- `GET /orders` - List orders for authenticated user
- `GET /orders/{id}` - Get order by ID
- `POST /orders` - Create a new order
- `PUT /orders/{id}` - Update order
- `DELETE /orders/{id}` - Delete order
- `GET /orders/{id}/status` - Get background job status

### System

- `POST /reset` - Reset all data to initial state
- `POST /reset?entity=products` - Reset specific entity
- `GET /health` - Health check

### Performance

- `GET /perf/payload?size=1kb` - Generate payload (1kb, 10kb, 100kb, 1mb)
- `GET /perf/payload?items=1000` - Generate N items
- `GET /perf/counter` - Get counter value
- `POST /perf/counter/increment` - Increment counter
- `POST /perf/counter/reset` - Reset counter

### Custom Collections (Dynamic Endpoints)

Create your own collections with any JSON structure. All simulation headers work automatically!

**CRUD Operations:**
- `GET /custom` - List all collection names
- `GET /custom/{collection}` - List entities in collection
- `GET /custom/{collection}/{id}` - Get entity by ID
- `POST /custom/{collection}` - Create entity
- `PUT /custom/{collection}/{id}` - Update entity
- `DELETE /custom/{collection}/{id}` - Delete entity

**Schema Validation:**
- `GET /custom/{collection}/_schema` - Get schema
- `POST /custom/{collection}/_schema` - Define JSON Schema
- `DELETE /custom/{collection}/_schema` - Remove schema

**Background Jobs:**
- `GET /custom/{collection}/_background` - Get config
- `POST /custom/{collection}/_background` - Configure background job
- `DELETE /custom/{collection}/_background` - Remove config

## Simulation Headers

All simulation headers work on **any endpoint**:

### Delay Simulation

```http
GET /products
X-Simulate-Delay: 500
```

### Error Simulation

```http
GET /products/1
X-Simulate-Error: true
```

### Retry Simulation

```http
GET /products/1
X-Simulate-Retry: 3
X-Request-Id: unique-id-123
```

First 2 requests return 500, third request succeeds.

### Chaos Failure Rate

```http
GET /products
X-Chaos-FailureRate: 0.3
```

30% chance of returning 500 error.

### Chaos Latency Range

```http
GET /products
X-Chaos-LatencyRange: 100-500
```

Random delay between 100-500ms.

### Background Job Delay Override

```http
POST /products
X-Background-Delay: 5000
Content-Type: application/json

{
  "name": "Test Product",
  "price": 100,
  "stockQuantity": 10,
  "category": "Electronics"
}
```

Overrides default background job delay.

## Examples

### Create Product with Background Job

```bash
# Create product
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -H "X-Background-Delay: 2000" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "stockQuantity": 50,
    "category": "Electronics"
  }'

# Check status (initially "processing")
curl http://localhost:5000/products/{id}/status

# After delay, CalculatedPrice will be set (Price * 1.2)
curl http://localhost:5000/products/{id}
```

### Create Order with Authentication

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: test-api-key-123" \
  -d '{
    "productIds": ["guid1", "guid2"],
    "totalAmount": 199.99
  }'
```

Order status will automatically progress: Pending → Processing → Completed

### Retry Scenario Testing

```bash
# First two requests fail, third succeeds
for i in {1..3}; do
  curl -X GET http://localhost:5000/products/123 \
    -H "X-Simulate-Retry: 3" \
    -H "X-Request-Id: test-retry-123"
done
```

### Performance Testing

```bash
# Generate 1MB payload
curl http://localhost:5000/perf/payload?size=1mb

# Generate 1000 items
curl http://localhost:5000/perf/payload?items=1000

# Thread-safe counter
curl http://localhost:5000/perf/counter
curl -X POST http://localhost:5000/perf/counter/increment
```

### Custom Collections

```bash
# Create a custom user entity
curl -X POST http://localhost:5000/custom/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "age": 30,
    "verified": false
  }'

# Define validation schema
curl -X POST http://localhost:5000/custom/users/_schema \
  -H "Content-Type: application/json" \
  -d '{
    "type": "object",
    "required": ["name", "email"],
    "properties": {
      "name": { "type": "string" },
      "email": { "type": "string", "format": "email" },
      "age": { "type": "integer", "minimum": 0 },
      "verified": { "type": "boolean" }
    }
  }'

# Configure background job for users (verification status progression)
curl -X POST http://localhost:5000/custom/users/_background \
  -H "Content-Type: application/json" \
  -d '{
    "fieldPath": "verified",
    "operation": "sequence:false,true",
    "delayMs": 2000
  }'

# Create a user - verified will be set to true after 2 seconds
curl -X POST http://localhost:5000/custom/users \
  -H "Content-Type: application/json" \
  -H "X-Background-Delay: 2000" \
  -d '{
    "name": "Jane Doe",
    "email": "jane@example.com",
    "age": 25,
    "verified": false
  }'

# Check user - initially verified=false
curl http://localhost:5000/custom/users/{id}

# After delay, verified will be automatically set to true
curl http://localhost:5000/custom/users/{id}

# Use with simulation headers!
curl http://localhost:5000/custom/users \
  -H "X-Simulate-Delay: 500" \
  -H "X-Simulate-Retry: 2" \
  -H "X-Request-Id: test-123"
```

**Background Job Operations:**
- `sequence:val1,val2,val3` - Cycles through values
- `sum:path.to.array.field` - Sum of values in array
- `count:path.to.array` - Count of items
- `timestamp` - Current UTC timestamp
- `random:min,max` - Random number

## Use Cases

### Integration Testing (TeaPie)

- Test retry logic with `X-Simulate-Retry`
- Test error handling with `X-Simulate-Error`
- Test async operations with background jobs
- Test authentication flows with Orders endpoints

### Benchmark Tools

- Test payload sizes with `/perf/payload`
- Test concurrent operations with `/perf/counter`
- Simulate latency with `X-Simulate-Delay`
- Test failure scenarios with `X-Chaos-FailureRate`

## OpenAPI Documentation

When running in Development mode, visit:
- Swagger UI: `http://localhost:5000/swagger`
- Scalar API Reference: `http://localhost:5000/scalar/v1`

## License

See LICENSE file for details.
