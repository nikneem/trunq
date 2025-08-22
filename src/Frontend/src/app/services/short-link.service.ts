import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { ConfigService } from './config.service';

export interface CreateLinkRequestDto {
  targetUrl: string;
}

export interface CreateLinkResponseDto {
  id: string;
  shortCode: string;
  targetUrl: string;
  shortUrl: string;
  createdAt: string;
}

export interface ShortLinkDetailsDto {
  id: string;
  shortCode: string;
  targetUrl: string;
  shortUrl?: string;
  hits: number;
  createdAt: string;
}

export interface UpdateLinkRequestDto {
  targetUrl: string;
  shortCode?: string;
}

@Injectable({ providedIn: 'root' })
export class ShortLinkService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);
  
  readonly lastCreated = signal<CreateLinkResponseDto | null>(null);
  readonly isCreating = signal(false);
  readonly isLoading = signal(false);
  readonly isUpdating = signal(false);
  readonly isDeleting = signal(false);
  readonly error = signal<string | null>(null);
  readonly shortLinks = signal<ShortLinkDetailsDto[]>([]);

  create(targetUrl: string): Observable<CreateLinkResponseDto> {
    this.isCreating.set(true);
    this.error.set(null);
    const body: CreateLinkRequestDto = { targetUrl };
    return this.http.post<CreateLinkResponseDto>(this.config.buildApiUrl('/api/links'), body).pipe(
      tap({
        next: (res) => {
          this.lastCreated.set(res);
          this.isCreating.set(false);
          // Add to the list if it exists
          const current = this.shortLinks();
          const newLink: ShortLinkDetailsDto = {
            id: res.id,
            shortCode: res.shortCode,
            targetUrl: res.targetUrl,
            hits: 0,
            createdAt: res.createdAt
          };
          this.shortLinks.set([newLink, ...current]);
        },
        error: (err) => {
          const msg = err?.error?.error || err?.error?.title || err.message || 'Creation failed';
          this.error.set(msg);
          this.isCreating.set(false);
        }
      })
    );
  }

  getAll(): Observable<ShortLinkDetailsDto[]> {
    this.isLoading.set(true);
    this.error.set(null);
    return this.http.get<ShortLinkDetailsDto[]>(this.config.buildApiUrl('/api/links')).pipe(
      tap({
        next: (links) => {
          this.shortLinks.set(links);
          this.isLoading.set(false);
        },
        error: (err) => {
          const msg = err?.error?.error || err?.error?.title || err.message || 'Failed to load links';
          this.error.set(msg);
          this.isLoading.set(false);
        }
      })
    );
  }

  getById(id: string): Observable<ShortLinkDetailsDto> {
    return this.http.get<ShortLinkDetailsDto>(this.config.buildApiUrl(`/api/links/${id}`));
  }

  update(id: string, targetUrl: string, shortCode?: string): Observable<ShortLinkDetailsDto> {
    this.isUpdating.set(true);
    this.error.set(null);
    const body: UpdateLinkRequestDto = { targetUrl, shortCode };
    return this.http.put<ShortLinkDetailsDto>(this.config.buildApiUrl(`/api/links/${id}`), body).pipe(
      tap({
        next: (updatedLink) => {
          this.isUpdating.set(false);
          // Update in the list
          const current = this.shortLinks();
          const updatedList = current.map(link => 
            link.id === id ? updatedLink : link
          );
          this.shortLinks.set(updatedList);
        },
        error: (err) => {
          const msg = err?.error?.error || err?.error?.title || err.message || 'Update failed';
          this.error.set(msg);
          this.isUpdating.set(false);
        }
      })
    );
  }

  delete(id: string): Observable<void> {
    this.isDeleting.set(true);
    this.error.set(null);
    return this.http.delete<void>(this.config.buildApiUrl(`/api/links/${id}`)).pipe(
      tap({
        next: () => {
          this.isDeleting.set(false);
          // Remove from the list
          const current = this.shortLinks();
          const filteredList = current.filter(link => link.id !== id);
          this.shortLinks.set(filteredList);
        },
        error: (err) => {
          const msg = err?.error?.error || err?.error?.title || err.message || 'Delete failed';
          this.error.set(msg);
          this.isDeleting.set(false);
        }
      })
    );
  }

  clearError(): void {
    this.error.set(null);
  }

  /**
   * Builds the full short URL using the backend hostname
   * Uses the shortUrl from the backend if available, otherwise builds it locally
   */
  buildShortUrl(shortCode: string, shortUrl?: string): string {
    return shortUrl || this.config.buildShortUrl(shortCode);
  }
}
