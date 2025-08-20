import { inject } from '@angular/core';
import { CanMatchFn, Router, UrlSegment, Route } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map, take } from 'rxjs/operators';

/**
 * Guard protecting lazy-loaded /app area.
 * Uses ID token authentication state; redirects to root if unauthenticated.
 */
export const isAuthenticatedGuard: CanMatchFn = (
  route: Route,
  segments: UrlSegment[]
) => {
  const oidc = inject(OidcSecurityService);
  const router = inject(Router);
  return oidc.isAuthenticated$.pipe(
    take(1),
    map(({ isAuthenticated }) => {
      if (isAuthenticated) return true;
      return router.createUrlTree(['/']);
    })
  );
};
