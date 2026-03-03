# MMLib.DummyApi

A dynamic mock REST API for integration testing, benchmarking, and UI
prototyping. Spin up realistic mock backends without writing a single line of
backend code.

## Quick Start

Pull and run:

```bash
docker pull ghcr.io/burgyn/mmlib-dummyapi
docker run -p 8080:8080 ghcr.io/burgyn/mmlib-dummyapi
```

The image ships with three collections (products, orders, customers) ready to
use. Try it:

```bash
curl http://localhost:8080/products
```

API documentation is available at [http://localhost:8080/scalar/v1](http://localhost:8080/scalar/v1).

## Default Collections

| Collection | Seed Count | Auth | Background Job |
| --- | --- | --- | --- |
| `products` | 50 | No | `calculatedPrice` (random 10–100) |
| `orders` | 20 | Yes (`X-Api-Key`) | `status` (pending→processing→completed) |
| `customers` | 30 | No | — |

Each collection exposes full CRUD: `GET /{collection}`,
`GET /{collection}/{id}`, `POST /{collection}`, `PUT /{collection}/{id}`,
`DELETE /{collection}/{id}`.

## Custom Collections

Provide your own `collections.json` by mounting it and setting the path:

```bash
docker run -p 8080:8080 \
  -v ./my-collections.json:/config/collections.json \
  -e DUMMYAPI__COLLECTIONSFILE=/config/collections.json \
  ghcr.io/burgyn/mmlib-dummyapi
```

### Minimal Example

```json
{
  "collections": [
    {
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
    }
  ]
}
```

### Collection Definition Reference

| Property | Type | Description |
| --- | --- | --- |
| `name` | string | URL path (required). Example: `products` → `/products` |
| `displayName` | string | Display name in OpenAPI docs |
| `description` | string | OpenAPI description |
| `authRequired` | boolean | Require `X-Api-Key` header (default: `false`) |
| `seedCount` | integer | Items to auto-generate at startup (default: 0) |
| `schema` | object | JSON Schema for validation and OpenAPI |
| `backgroundJob` | object | Async field updates (see [Background Jobs](#background-jobs)) |
| `rules` | array | Response rules (see [Response Rules](#response-rules)) |

## JSON Schema and Smart Data Generation

The `schema` property uses standard [JSON Schema](https://json-schema.org/).
It drives both validation (POST/PUT) and OpenAPI documentation.

Field names in your schema are mapped to realistic fake data. Use common names
and get realistic values automatically:

| Category | Field Names | Generated Data |
| --- | --- | --- |
| Personal | `firstName`, `lastName`, `fullName`, `name` | Names |
| Contact | `email`, `phone`, `username` | Email, phone, username |
| Address | `address`, `city`, `country`, `zipCode`, `state` | Address components |
| Company | `company`, `department`, `jobTitle` | Company data |
| Product | `productName`, `price`, `category`, `sku`, `color` | Commerce data |
| Content | `description`, `content`, `comment` | Lorem ipsum text |
| Dates | `createdAt`, `updatedAt`, `birthDate`, `date` | Timestamps |
| IDs | `customerId`, `orderId`, `userId` | GUIDs |
| Other | `status`, `url`, `imageUrl`, `quantity` | Status, URLs, numbers |

Schema formats (`email`, `uri`, `uuid`, `date`, `date-time`) and `enum` arrays
are also respected.

### Validation Restrictions

The schema validates POST and PUT requests. Invalid payloads return 400 with
error details. Use standard JSON Schema keywords:

```json
{
  "schema": {
    "type": "object",
    "required": ["name", "price", "status"],
    "properties": {
      "name": {
        "type": "string",
        "minLength": 2,
        "maxLength": 100
      },
      "price": {
        "type": "number",
        "minimum": 0.01,
        "maximum": 999999.99
      },
      "status": {
        "type": "string",
        "enum": ["draft", "active", "archived"]
      },
      "email": {
        "type": "string",
        "format": "email"
      },
      "phone": {
        "type": "string",
        "pattern": "^[+]?[0-9\\s.-]{9,20}$"
      }
    }
  }
}
```

| Keyword | Applies to | Description |
| --- | --- | --- |
| `required` | object | Array of required property names |
| `minLength`, `maxLength` | string | Character limits |
| `minimum`, `maximum` | number, integer | Numeric bounds |
| `format` | string | `email`, `uri`, `uuid`, `date`, `date-time` |
| `pattern` | string | Regex for validation |
| `enum` | any | Allowed values |

## Response Rules

Define mockoon-style rules to return custom responses when conditions match.
Rules are evaluated before CRUD; the first matching rule (by priority) wins.

### Rule Structure

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
        "headers": { "X-Custom": "value" },
        "delayMs": 500
      }
    }
  ]
}
```

### Condition Sources

| Source | Description |
| --- | --- |
| `query` | Query string parameter |
| `header` | Request header |
| `body` | JSON path in request body (e.g. `user.name`) |
| `path` | Route parameter |

### Operators

`equals`, `contains`, `startsWith`, `endsWith`, `greaterThan`, `lessThan`,
`range`, `exists`, `notExists`

### Response Properties

| Property | Type | Description |
| --- | --- | --- |
| `statusCode` | integer | HTTP status (default: 200) |
| `body` | object/array | Response body |
| `headers` | object | Custom response headers |
| `delayMs` | integer | Delay before response (ms) |

Use `method: "*"` to match any HTTP method.

## Background Jobs

Simulate async processing: after a POST creates an entity, a background job can
update a field after a delay.

### Configuration

```json
{
  "backgroundJob": {
    "fieldPath": "status",
    "operation": "sequence:pending,processing,completed",
    "delayMs": 3000
  }
}
```

| Property | Description |
| --- | --- |
| `fieldPath` | Field to update (e.g. `status`, `calculatedPrice`) |
| `operation` | Operation to apply (see below) |
| `delayMs` | Delay before job runs (ms) |

### Operations

| Operation | Format | Example |
| --- | --- | --- |
| `sequence` | `sequence:val1,val2,val3` | `sequence:pending,processing,completed` |
| `sum` | `sum:path.to.array.field` | `sum:items.price` |
| `count` | `count:path.to.array` | `count:items` |
| `timestamp` | `timestamp` | Current UTC time |
| `random` | `random:min,max` | `random:10,100` |

Sequence jobs advance one value per run. Override delay per request with the
`X-Background-Delay` header.

## Simulation Headers

Apply to any endpoint for testing retries, latency, and chaos:

| Header | Description |
| --- | --- |
| `X-Simulate-Delay: 500` | Add 500 ms delay |
| `X-Simulate-Error: true` | Return 500 error |
| `X-Simulate-Retry: 3` | Fail first 2 requests, succeed on 3rd |
| `X-Request-Id: unique-id` | Track retries (required with `X-Simulate-Retry`) |
| `X-Chaos-FailureRate: 0.3` | 30% chance of 500 |
| `X-Chaos-LatencyRange: 100-500` | Random delay in range (ms) |
| `X-Background-Delay: 5000` | Override background job delay |

## API Key Authentication

Set `authRequired: true` on a collection to require the `X-Api-Key` header.
Default key: `test-api-key-123`. Configure via `DUMMYAPI__DEFAULTAPIKEY`.

```bash
curl -H "X-Api-Key: test-api-key-123" http://localhost:8080/orders
```

## API Endpoints

### Dynamic CRUD (per collection)

- `GET /{collection}` — List all
- `GET /{collection}/{id}` — Get by ID
- `POST /{collection}` — Create
- `PUT /{collection}/{id}` — Update
- `DELETE /{collection}/{id}` — Delete

### Collection Management

- `GET /custom/_definitions` — List definitions
- `GET /custom/_definitions/{name}` — Get definition
- `POST /custom/_definitions` — Create collection
- `PUT /custom/_definitions/{name}` — Update definition
- `DELETE /custom/_definitions/{name}` — Delete collection

### System

- `POST /reset` — Reset all collections
- `POST /reset?collection=products` — Reset specific collection
- `GET /health` — Health check

### Performance

- `GET /perf/payload?size=1mb` — Generate payload
- `GET /perf/counter` — Get counter
- `POST /perf/counter/increment` — Increment counter

## Environment Variables

| Variable | Description |
| --- | --- |
| `DUMMYAPI__COLLECTIONSFILE` | Path to collections JSON file |
| `DUMMYAPI__DEFAULTAPIKEY` | API key (default: `test-api-key-123`) |
| `DUMMYAPI__PERFORMANCE__MAXPAYLOADSIZEMB` | Max payload for `/perf/payload` (default: 10) |

## License

MIT License.
