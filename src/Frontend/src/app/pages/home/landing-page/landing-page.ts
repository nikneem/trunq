import { Component, signal, effect } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatDividerModule } from '@angular/material/divider';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ShortLinkService } from '../../../services/short-link.service';

@Component({
  selector: 'trq-landing-page',
  imports: [CommonModule, MatButtonModule, MatIconModule, MatCardModule, MatInputModule, MatFormFieldModule, MatToolbarModule, MatDividerModule, RouterModule],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss'
})
export class LandingPage {
  readonly features = [
    { icon: 'link', title: 'Shorten Any URL', description: 'Turn long, unwieldy links into clean, shareable trunqs in a flash.' },
    { icon: 'speed', title: 'Fast Resolution', description: 'Ultra-low latency redirects powered by a lightweight, scalable backend.' },
    { icon: 'security', title: 'Safe & Sanitized', description: 'All input validated. No surprises. No hidden tracking parameters added.' },
    { icon: 'insights', title: 'Future Analytics', description: 'Designed to evolve with click insights and engagement metrics.' }
  ];

  readonly sellingPoints = [
    'Opaque, collision-resistant codes',
    'Secure handling of user input',
    'Extensible foundation for custom domains & analytics'
  ];

  readonly year = new Date().getFullYear();
  readonly isAuthenticated = signal(false);

  constructor(
    private readonly oidcSecurityService: OidcSecurityService,
    private readonly router: Router,
    private readonly shortLinkService: ShortLinkService
  ) {
    this.oidcSecurityService.isAuthenticated$.subscribe(result => {
      this.isAuthenticated.set(result.isAuthenticated)
    });

    // Reset any existing created link & errors upon logout
    effect(() => {
      if (!this.isAuthenticated()) {
        this.shortLinkService.lastCreated.set(null);
        this.shortLinkService.error.set(null);
      }
    });
  }

  // Property-style access returning signal values for template convenience
  get createdLink() { return this.shortLinkService.lastCreated(); }
  get createError() { return this.shortLinkService.error(); }
  get isCreating() { return this.shortLinkService.isCreating(); }

  goLinks() {
    if (!this.isAuthenticated()) return;
    this.router.navigate(['/app','links']);
  }

  login() {
    // Initiates the OIDC authorization flow.
    this.oidcSecurityService.authorize();
  }

  shortenUrl(url: string) {
    if (!this.isAuthenticated()) return;
    if (!url || !/^https?:\/\//i.test(url)) {
      this.shortLinkService.error.set('Enter a valid http(s) URL');
      return;
    }
    this.shortLinkService.create(url).subscribe();
  }

  isValidUrl(url: string | null | undefined): boolean {
    if (!url) return false;
    return /^https?:\/\//i.test(url.trim());
  }

}
