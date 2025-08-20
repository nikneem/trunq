# Trunq API - Short Link Creation Endpoint

## Overview
The `/api/links` POST endpoint creates short links from target URLs with OpenID Connect authentication using Keycloak.

## Prerequisites

### 1. Keycloak Setup
- Keycloak running at `http://localhost:8080`
- Realm named `trunq` configured
- Client configured with appropriate access

### 2. Azure Table Storage
- Azurite (Azure Storage Emulator) running locally
- Connection string: `UseDevelopmentStorage=true`

## API Usage

### Authentication
The endpoint requires a valid JWT Bearer token from Keycloak:

```http
Authorization: Bearer <your-jwt-token>
```

### Request
```http
POST /api/links
Content-Type: application/json

{
  "targetUrl": "https://example.com/very/long/url/path"
}
```

### Response (201 Created)
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "shortCode": "abc123",
  "targetUrl": "https://example.com/very/long/url/path",
  "createdAt": "2023-12-01T10:30:00.000Z"
}
```

### Error Responses

#### 400 Bad Request - Invalid URL
```json
{
  "error": "Target URL must be a valid HTTP or HTTPS URL."
}
```

#### 401 Unauthorized
Missing or invalid JWT token.

#### 500 Internal Server Error
```json
{
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Failed to generate unique short code after multiple attempts."
}
```

## Features

### URL Validation
- Must be valid HTTP/HTTPS URL
- Uses `Uri.TryCreate()` for validation

### Short Code Generation
- 6-character alphanumeric codes (a-z, 0-9)
- Collision detection with retry mechanism
- Maximum 5 retry attempts

### User Association
- Each short link is associated with the authenticated user's subject ID
- Subject ID extracted from JWT token claims

### Azure Table Storage
- Partition key: "shortlink"
- Row key: Short code (for fast lookups)
- Automatic table creation if not exists

## Testing with curl

First, obtain a JWT token from Keycloak, then:

```bash
curl -X POST http://localhost:5000/api/links \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"targetUrl":"https://example.com"}'
```

## Development Notes

- Built with .NET 9 and C# 13
- Uses minimal APIs
- Follows CQRS pattern with command handlers
- Repository pattern for data access
- Structured logging with correlation IDs