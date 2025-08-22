# Trunq Configuration Guide

## Backend Configuration

### Location
The backend configuration is centralized in the `appsettings.json` files:
- `src/Backend/HexMaster.Trunq.Api/appsettings.json` (base configuration)
- `src/Backend/HexMaster.Trunq.Api/appsettings.Development.json` (development overrides)

### Configuration Structure
```json
{
  "Trunq": {
    "ShortUrlBaseUrl": "https://localhost:7277"
  }
}
```

### Usage
- The `TrunqOptions` class provides strongly-typed access to configuration
- The `IUrlBuilderService` uses this configuration to build consistent short URLs
- All API responses include the full short URL using the configured base URL

### Environment-Specific Settings
- **Development**: `https://localhost:7277` (same as API for local development)
- **Production**: Should be set to your short link domain (e.g., `https://trunq.link`)

## Frontend Configuration

### Environment Files
- `src/Frontend/src/environments/environment.ts` (development)
- `src/Frontend/src/environments/environment.production.ts` (production)

### Configuration Structure
```typescript
export interface Environment {
  production: boolean;
  apiBaseUrl: string;
  shortLinkBaseUrl: string;
}
```

### Development Configuration
```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7277',
  shortLinkBaseUrl: 'https://localhost:7277'
};
```

### Production Configuration
```typescript
export const environment = {
  production: true,
  apiBaseUrl: 'https://api.trunq.com',
  shortLinkBaseUrl: 'https://trunq.link'
};
```

## Services Using Configuration

### Backend Services
1. **TrunqOptions**: Strongly-typed configuration binding
2. **UrlBuilderService**: Builds short URLs consistently
3. **WebApplicationExtensions**: Uses URL builder in API responses

### Frontend Services
1. **ConfigService**: Central access to environment configuration
2. **ShortLinkService**: Uses config for API calls and URL building
3. **AuthConfig**: Uses environment config for secure routes

## Deployment Configuration

### Development
- Both frontend and backend use `localhost:7277`
- Short links work locally for testing
- No additional DNS configuration required

### Production
- **API Backend**: Deploy to `https://api.trunq.com`
- **Short Link Domain**: Configure DNS for `https://trunq.link`
- **Frontend**: Can be deployed to any domain (e.g., `https://app.trunq.com`)
- **Configuration**: Update environment files before build

### Key Benefits
1. **Single Source of Truth**: Each environment has one place to configure hostnames
2. **Consistent URLs**: All short URLs use the same base hostname
3. **Environment Separation**: Different URLs for development/production
4. **Easy Deployment**: Change configuration without code changes

## Configuration Validation

The backend includes configuration validation:
- Required fields are enforced
- Configuration is validated at startup
- Invalid configuration prevents application start

## Migration from Hardcoded URLs

All hardcoded URLs have been replaced with:
- Backend: `IUrlBuilderService.BuildShortUrl(shortCode)`
- Frontend: `ConfigService.buildShortUrl(shortCode)` or `ConfigService.buildApiUrl(endpoint)`

This ensures all URLs are consistent and configurable across environments.
