import { PassedInitialConfig } from 'angular-auth-oidc-client';

export const authConfig: PassedInitialConfig = {
  config: {
    // Keycloak realm issuer (matches well-known 'issuer')
    authority: 'http://localhost:8080/realms/trunq',
    // Scopes needed (openid mandatory). Add others later when API requires them.
    scope: 'openid profile offline_access',
    clientId: 'angular-client-app',
    redirectUrl: window.location.origin,
    // Use home page for post logout; Keycloak will append state if needed
    postLogoutRedirectUri: window.location.origin,
    responseType: 'code', // Authorization Code + PKCE
    silentRenew: true,
    useRefreshToken: true, // Keycloak supports refresh tokens; avoids iframe silent renew complexities
    renewTimeBeforeTokenExpiresInSeconds: 30,
    // Secure API base URLs that should receive the Authorization: Bearer <token>
    secureRoutes: ['https://localhost:7277/']
    // Keep default security-related settings; do not relax validation in production.
  }
};
