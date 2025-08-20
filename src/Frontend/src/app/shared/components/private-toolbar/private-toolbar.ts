import { Component, computed, signal } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'trq-private-toolbar',
  imports: [MatToolbarModule, MatButtonModule, MatMenuModule, MatIconModule, MatDividerModule, RouterModule],
  templateUrl: './private-toolbar.html',
  styleUrl: './private-toolbar.scss'
})
export class PrivateToolbar {
  private readonly idTokenClaims = signal<Record<string, any> | null>(null);

  readonly displayName = computed(() => {
    const claims = this.idTokenClaims();
    if (!claims) return 'Account';
    return (
      claims['name'] ||
      claims['preferred_username'] ||
      (claims['sub'] ? String(claims['sub']).substring(0, 8) : 'Account')
    );
  });

  constructor(private readonly oidcSecurityService: OidcSecurityService, private readonly router: Router) {
    this.oidcSecurityService.isAuthenticated$.subscribe(result => {
      if (result.isAuthenticated) {
        this.oidcSecurityService.getIdToken().subscribe(t => {
          this.idTokenClaims.set(this.decodeJwt(t));
        });
      } else {
        this.idTokenClaims.set(null);
      }
    });
  }

  private decodeJwt(token: string | null): Record<string, any> | null {
    if (!token) return null;
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + (4 - (base64.length % 4)) % 4, '=');
      const json = atob(padded);
      return JSON.parse(json);
    } catch {
      return null;
    }
  }

  goPreferences() {
    this.router.navigate(['/app/preferences']);
  }

  logout() {
    // Standard logoff; keeps alignment with provided postLogoutRedirect in config
    this.oidcSecurityService.logoff().subscribe();
  }

}
