import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private readonly config = environment;

  get apiBaseUrl(): string {
    return this.config.apiBaseUrl;
  }

  get shortLinkBaseUrl(): string {
    return this.config.shortLinkBaseUrl;
  }

  get isProduction(): boolean {
    return this.config.production;
  }

  /**
   * Builds the full short URL using the configured backend hostname
   */
  buildShortUrl(shortCode: string): string {
    return `${this.shortLinkBaseUrl}/${shortCode}`;
  }

  /**
   * Builds API endpoint URLs
   */
  buildApiUrl(endpoint: string): string {
    const baseUrl = this.apiBaseUrl.endsWith('/') 
      ? this.apiBaseUrl.slice(0, -1) 
      : this.apiBaseUrl;
    const cleanEndpoint = endpoint.startsWith('/') 
      ? endpoint 
      : `/${endpoint}`;
    return `${baseUrl}${cleanEndpoint}`;
  }
}
