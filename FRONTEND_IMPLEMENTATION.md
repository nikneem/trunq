# Short Links CRUD Frontend Implementation

## Overview

This implementation provides a complete CRUD (Create, Read, Update, Delete) interface for managing Short Links using Angular Material components. The interface is fully responsive and optimized for mobile devices, tablets, and desktop computers.

## Centralized Configuration

### Backend Configuration
The backend hostname and short URL base are configured centrally in `appsettings.json`:

```json
{
  "Trunq": {
    "ShortUrlBaseUrl": "https://localhost:7277"
  }
}
```

This configuration is used by:
- `TrunqOptions` class for strongly-typed configuration
- `UrlBuilderService` to build short URLs consistently
- All API endpoints that return short URLs

### Frontend Configuration
The frontend uses environment-specific configuration files:

**Development** (`src/environments/environment.ts`):
```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7277',
  shortLinkBaseUrl: 'https://localhost:7277'
};
```

**Production** (`src/environments/environment.production.ts`):
```typescript
export const environment = {
  production: true,
  apiBaseUrl: 'https://api.trunq.com',
  shortLinkBaseUrl: 'https://trunq.link'
};
```

### Centralized Services
- **ConfigService**: Provides centralized access to environment configuration
- **UrlBuilderService** (Backend): Builds short URLs using backend hostname
- **ShortLinkService** (Frontend): Uses centralized config for API calls and URL building

## Short Link URL Format

Short links now follow the format: `{backend-hostname}/{shortcode}`

Examples:
- Development: `https://localhost:7277/abc123`
- Production: `https://trunq.link/abc123`

The backend:
1. Serves short links directly at `/{shortcode}` (public endpoint)
2. Returns full short URLs in API responses
3. Uses centralized configuration for URL building

The frontend:
1. Uses the backend hostname for short URLs (not frontend origin)
2. Can fall back to local URL building if backend doesn't provide shortUrl
3. All copy-to-clipboard actions use the backend hostname

## Features Implemented

### 1. Backend API Enhancement
- Enhanced the existing UPDATE operation to support updating short codes
- Added validation to prevent duplicate short codes
- All CRUD operations are now available:
  - `POST /api/links` - Create new short link
  - `GET /api/links` - Get all user's short links
  - `GET /api/links/{id}` - Get specific short link
  - `PUT /api/links/{id}` - Update short link (URL and/or short code)
  - `DELETE /api/links/{id}` - Delete short link

### 2. Frontend Service
- **ShortLinkService**: Complete service with all CRUD operations
- Reactive state management using Angular signals
- Automatic error handling and loading states
- Real-time list updates after operations

### 3. Responsive UI Components

#### Desktop/Laptop View
- Full data table with columns: Short Code, Target URL, Hits, Created Date, Actions
- Header with search functionality and create button
- Context menus for each row with copy, edit, delete actions

#### Tablet View  
- Compact table showing: Short Code, Target URL, Actions
- Optimized for touch interaction
- Same functionality as desktop in a more compact format

#### Mobile View
- Card-based layout for better mobile experience
- Each link displayed as a Material Card with:
  - Prominent short code chip (clickable to copy)
  - Target URL with external link indicator
  - Stats (hits and creation date) with icons
  - Action menu for edit/delete operations
- Floating Action Button (FAB) for quick link creation
- Optimized touch targets and gestures

### 4. Modal Dialogs

#### Create Link Dialog
- Simple form with URL input and validation
- Real-time validation feedback
- Loading states during creation

#### Edit Link Dialog  
- Edit both target URL and short code
- Form validation for both fields
- Duplicate short code detection
- Loading states and error handling

#### Delete Confirmation Dialog
- Shows link details before deletion
- Clear warning about permanent action
- Loading states during deletion

### 5. User Experience Features

#### Copy to Clipboard
- One-click copy functionality for short codes
- Generates full URL (e.g., `https://domain.com/abc123`)
- Visual feedback with snackbar notifications

#### Search & Filter
- Real-time search across short codes and target URLs
- Case-insensitive matching
- Clear search functionality

#### Notifications
- Success messages for create/edit/delete operations
- Error messages with helpful context
- Non-intrusive snackbar notifications

#### Loading States
- Progress indicators during API operations
- Disabled buttons during processing
- Skeleton loading for better perceived performance

#### Empty States
- Helpful messaging when no links exist
- Search-specific empty states
- Call-to-action buttons for first-time users

## Technical Implementation

### Responsive Design
```typescript
// Breakpoint detection using Angular CDK
this.breakpointObserver.observe([
  Breakpoints.HandsetPortrait,
  Breakpoints.HandsetLandscape
]).subscribe(result => {
  this.isMobile.set(result.matches);
});
```

### State Management
```typescript
// Reactive signals for efficient updates
readonly shortLinks = signal<ShortLinkDetailsDto[]>([]);
readonly filteredLinks = computed(() => {
  const links = this.allLinks();
  const search = this.searchTerm().toLowerCase();
  return search ? links.filter(/* search logic */) : links;
});
```

### Form Validation
```typescript
// Custom URL validator
private urlValidator(control: any) {
  const value = control.value;
  if (!value) return null;
  
  try {
    new URL(value);
    return null;
  } catch {
    return { url: true };
  }
}
```

## File Structure

```
src/app/pages/private/links-list/
├── links-list.ts                    # Main component
├── links-list.html                  # Template
├── links-list.scss                  # Styles
├── create-link-dialog/
│   └── create-link-dialog.ts       # Create dialog
├── edit-link-dialog/
│   └── edit-link-dialog.ts         # Edit dialog
└── delete-link-dialog/
    └── delete-link-dialog.ts        # Delete confirmation
```

## Usage

The links list is available at `/private/links` route for authenticated users. Users can:

1. **View all their short links** in a responsive layout
2. **Create new links** using the create button or FAB
3. **Edit existing links** (both URL and short code)
4. **Delete links** with confirmation
5. **Copy links** to clipboard with one click
6. **Search/filter** their links in real-time

## Browser Support

- Modern browsers with ES2020+ support
- Mobile Safari (iOS 12+)
- Chrome Mobile (Android 8+)
- Desktop Chrome, Firefox, Safari, Edge

## Dependencies

- Angular 20.1+
- Angular Material 20.1+
- Angular CDK (Layout module)
- RxJS 7.8+

The implementation provides a complete, production-ready interface that works seamlessly across all device types and screen resolutions.
