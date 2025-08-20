import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

// API base URL (development). Replace with environment-specific config when available.
const API_BASE = 'https://localhost:7277'; // Matches secureRoutes in auth config

export interface CreateLinkRequestDto {
  targetUrl: string;
}

export interface CreateLinkResponseDto {
  id: string;
  shortCode: string;
  targetUrl: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class ShortLinkService {
  private readonly http = inject(HttpClient);
  readonly lastCreated = signal<CreateLinkResponseDto | null>(null);
  readonly isCreating = signal(false);
  readonly error = signal<string | null>(null);

  create(targetUrl: string): Observable<CreateLinkResponseDto> {
    this.isCreating.set(true);
    this.error.set(null);
    const body: CreateLinkRequestDto = { targetUrl };
    return this.http.post<CreateLinkResponseDto>(`${API_BASE}/api/links`, body).pipe(
      tap({
        next: (res) => {
          this.lastCreated.set(res);
          this.isCreating.set(false);
        },
        error: (err) => {
          const msg = err?.error?.error || err?.error?.title || err.message || 'Creation failed';
            this.error.set(msg);
            this.isCreating.set(false);
        }
      })
    );
  }
}
