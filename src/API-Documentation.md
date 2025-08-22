# Trunq API - Complete Short Link CRUD Operations

## Overview
The Trunq API provides complete CRUD (Create, Read, Update, Delete) operations for short links with OpenID Connect authentication using Keycloak. Each operation validates the Subject ID in the JWT token to ensure users can only manage their own short links.

## Interactive API Documentation
The API includes interactive documentation powered by **Scalar** which provides a beautiful, modern interface for exploring and testing the API endpoints.

- **Development URL**: `https://localhost:7277/scalar/v1`
- **Features**: Interactive request/response examples, authentication support, and live API testing
- **Theme**: Purple theme for enhanced visual experience

## Architecture
The implementation uses .NET Aspire for service orchestration and follows CQRS patterns with clean separation of concerns:
- **Aspire Integration**: Azure Table Storage configured through Aspire AppHost with automatic Azurite setup for local development
- **CQRS Pattern**: Commands and Queries with dedicated handlers for each operation
- **Service Discovery**: Aspire manages service configuration and connections
- **Domain-Driven Design**: Rich domain models with business logic encapsulation
- **Interactive Documentation**: Scalar UI for comprehensive API exploration

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

### Accessing API Documentation
When running in development mode, the interactive Scalar documentation is automatically available at:
- **HTTPS**: `https://localhost:7277/scalar/v1`
- **HTTP**: `http://localhost:5029/scalar/v1`

The Scalar interface will automatically launch when debugging the API project.

## API Endpoints

All endpoints require JWT Bearer token authentication and automatically validate ownership based on the Subject ID in the token.

### Authentication
All endpoints require a valid JWT Bearer token from Keycloak:

```http
Authorization: Bearer <your-jwt-token>
```

### 1. Create Short Link
Creates a new short link for the authenticated user.

```http
POST /api/links
Content-Type: application/json

{
  "targetUrl": "https://example.com/very/long/url/path"
}
```

**Response (201 Created):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "shortCode": "abc123",
  "targetUrl": "https://example.com/very/long/url/path",
  "createdAt": "2023-12-01T10:30:00.000Z"
}
```

### 2. Get All User's Short Links
Retrieves all short links belonging to the authenticated user.

```http
GET /api/links
```

**Response (200 OK):**
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "shortCode": "abc123",
    "targetUrl": "https://example.com/very/long/url/path",
    "hits": 42,
    "createdAt": "2023-12-01T10:30:00.000Z"
  },
  {
    "id": "987fcdeb-51d2-43b7-a890-123456789abc",
    "shortCode": "xyz789",
    "targetUrl": "https://another-example.com/path",
    "hits": 15,
    "createdAt": "2023-12-01T09:15:00.000Z"
  }
]
```

### 3. Get Short Link by ID
Retrieves a specific short link by its ID (only if owned by the authenticated user).

```http
GET /api/links/{id}
```

**Response (200 OK):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "shortCode": "abc123",
  "targetUrl": "https://example.com/very/long/url/path",
  "hits": 42,
  "createdAt": "2023-12-01T10:30:00.000Z"
}
```

### 4. Update Short Link
Updates the target URL of an existing short link (only if owned by the authenticated user).

```http
PUT /api/links/{id}
Content-Type: application/json

{
  "targetUrl": "https://updated-example.com/new/path"
}
```

**Response (200 OK):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "shortCode": "abc123",
  "targetUrl": "https://updated-example.com/new/path",
  "hits": 42,
  "createdAt": "2023-12-01T10:30:00.000Z"
}
```

### 5. Delete Short Link
Deletes a short link (only if owned by the authenticated user).

```http
DELETE /api/links/{id}
```

**Response (204 No Content):**
Empty response body on successful deletion.

## Error Responses

### 400 Bad Request - Invalid URL
```json
{
  "error": "Target URL must be a valid HTTP or HTTPS URL."
}
```

### 401 Unauthorized
Missing or invalid JWT token.

### 403 Forbidden
Attempting to access/modify another user's short links.

### 404 Not Found
```json
{
  "error": "Short link with ID '123e4567-e89b-12d3-a456-426614174000' not found."
}
```

### 500 Internal Server Error
```json
{
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Failed to generate unique short code after multiple attempts."
}
```

## Security Features

### Subject ID Validation
- Each operation extracts the Subject ID from the JWT token claims
- All operations validate that the user can only access/modify their own short links
- The system checks both `ClaimTypes.NameIdentifier` and `"sub"` claims for compatibility

### URL Validation
- Target URLs must be valid HTTP/HTTPS URLs
- Uses `Uri.TryCreate()` for comprehensive validation
- Prevents injection attacks through URL normalization

### Authorization
- All endpoints require authentication
- Read operations validate ownership before returning data
- Write operations (create, update, delete) validate ownership before making changes

## Features

### Short Code Generation
- 6-character alphanumeric codes (a-z, 0-9)
- Collision detection with repository query approach
- Maximum 5 retry attempts for uniqueness

### Domain Model Validation
- Rich domain models with encapsulated business logic
- URL validation within domain objects
- Immutable creation timestamps and IDs

### Azure Table Storage Integration
- **Partition key**: "shortlink"
- **Row key**: Short code (for fast lookups)
- **Automatic table creation** if not exists
- **Aspire-managed connection**: No manual connection string configuration needed
- **Query optimization**: Efficient lookups by ID, short code, and subject ID

## CQRS Implementation

### Commands
- `CreateShortLinkCommand`: Creates new short links with collision handling
- `UpdateShortLinkCommand`: Updates existing short links with ownership validation
- `DeleteShortLinkCommand`: Deletes short links with ownership validation

### Queries
- `GetShortLinkByIdQuery`: Retrieves single short link by ID with ownership validation
- `GetUserShortLinksQuery`: Retrieves all short links for authenticated user

## Testing the API

### Using Scalar Interactive Documentation
1. Navigate to `https://localhost:7277/scalar/v1` (or the HTTP equivalent)
2. Use the interactive interface to explore endpoints
3. Add your JWT Bearer token using the "Authorize" button
4. Execute requests directly from the documentation interface

### Using curl Examples

#### Create a Short Link
```bash
curl -X POST https://localhost:7277/api/links \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"targetUrl":"https://example.com"}'
```

#### Get All Your Short Links
```bash
curl -X GET https://localhost:7277/api/links \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Get Specific Short Link
```bash
curl -X GET https://localhost:7277/api/links/123e4567-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Update a Short Link
```bash
curl -X PUT https://localhost:7277/api/links/123e4567-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"targetUrl":"https://updated-example.com"}'
```

#### Delete a Short Link
```bash
curl -X DELETE https://localhost:7277/api/links/123e4567-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Development Notes

- Built with .NET 9 and C# 13
- **Complete CRUD operations** with proper HTTP semantics
- **Interactive API Documentation** with Scalar for enhanced developer experience
- **Ownership validation** prevents unauthorized access to other users' short links
- **Aspire orchestration** for local development and testing
- Uses minimal APIs with route groups for clean organization
- Follows CQRS pattern with dedicated command and query handlers
- Repository pattern with Azure Table Storage implementation
- **Built-in observability** with structured logging and OpenTelemetry integration
- Extension methods provide clean separation of concerns

## Production Deployment

For production, replace the Aspire Azure Storage resource configuration with actual Azure Storage account details. Aspire supports environment-specific configuration for seamless deployment. The Scalar documentation interface is only available in development mode for security.