# Trunq API - Short Link Creation Endpoint

## Overview
The `/api/links` POST endpoint creates short links from target URLs with OpenID Connect authentication using Keycloak. The application is orchestrated using .NET Aspire with Azure Table Storage integration.

## Architecture
The implementation uses .NET Aspire for service orchestration and extension methods to cleanly separate concerns:
- **Aspire Integration**: Azure Table Storage configured through Aspire AppHost with automatic Azurite setup for local development
- `ServiceCollectionExtensions.AddShortLinkServices()` - Registers repositories and command handlers
- `WebApplicationExtensions.MapShortLinkEndpoints()` - Maps the API endpoints
- **Service Discovery**: Aspire manages service configuration and connections

## Prerequisites

### 1. .NET Aspire
- .NET Aspire SDK installed
- Run the solution through the Aspire AppHost for proper service orchestration

### 2. Keycloak Setup
- Keycloak running at `http://localhost:8080` (configured through Aspire)
- Realm named `trunq` configured
- Client configured with appropriate access

### 3. Azure Table Storage
- **Local Development**: Azurite automatically started by Aspire
- **Production**: Configure Azure Storage connection through Aspire configuration
- Table named `shortlinks` automatically created

## Running the Application

### Development Mode
```bash
# Navigate to the Aspire AppHost directory
cd src/Aspire/HexMaster.Trunq.Aspire/HexMaster.Trunq.Aspire.AppHost

# Run the Aspire AppHost (starts all services including Azurite)
dotnet run
```

The Aspire dashboard will be available at the URL shown in the console, typically `https://localhost:17000`.

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
- Collision detection with repository query approach
- Maximum 5 retry attempts

### User Association
- Each short link is associated with the authenticated user's subject ID
- Subject ID extracted from JWT token claims

### Azure Table Storage Integration
- **Partition key**: "shortlink"
- **Row key**: Short code (for fast lookups)
- **Automatic table creation** if not exists
- **Aspire-managed connection**: No manual connection string configuration needed
- **Local development**: Uses Azurite (Azure Storage Emulator) automatically
- **Production**: Configured through Aspire Azure Storage resource

## Aspire Benefits

- **Service Orchestration**: All dependencies (Keycloak, Azurite, PostgreSQL) started automatically
- **Configuration Management**: Connection strings and service discovery handled automatically
- **Observability**: Built-in telemetry, logging, and health checks
- **Development Experience**: Single command starts entire application stack
- **Resource Management**: Automatic container lifecycle management

## Testing with curl

With Aspire running, first obtain a JWT token from Keycloak, then:

```bash
curl -X POST https://localhost:7000/api/links \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"targetUrl":"https://example.com"}'
```

*Note: The actual port will be shown in the Aspire dashboard*

## Development Notes

- Built with .NET 9 and C# 13
- **Aspire orchestration** for local development and testing
- Uses minimal APIs with extension methods for clean organization
- Follows CQRS pattern with command handlers
- Repository pattern for data access
- **Automatic service discovery** and configuration through Aspire
- **Built-in observability** with OpenTelemetry integration
- Extension methods provide clean separation of concerns

## Production Deployment

For production, replace the Aspire Azure Storage resource configuration with actual Azure Storage account details. Aspire supports environment-specific configuration for seamless deployment.