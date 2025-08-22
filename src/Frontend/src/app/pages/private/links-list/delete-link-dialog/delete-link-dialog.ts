import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { ShortLinkDetailsDto, ShortLinkService } from '../../../../services/short-link.service';

export interface DeleteLinkDialogData {
  link: ShortLinkDetailsDto;
}

@Component({
  selector: 'trq-delete-link-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatProgressBarModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon color="warn">warning</mat-icon>
      Delete Short Link
    </h2>
    
    <mat-dialog-content>
      <p>Are you sure you want to delete this short link?</p>
      
      <div class="link-info">
        <strong>Short Code:</strong> {{ data.link.shortCode }}<br>
        <strong>Target URL:</strong> {{ data.link.targetUrl }}<br>
        <strong>Hits:</strong> {{ data.link.hits }}
      </div>
      
      <p class="warning-text">
        <mat-icon>info</mat-icon>
        This action cannot be undone. The short link will stop working immediately.
      </p>
      
      @if (shortLinkService.isDeleting()) {
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      }
      
      @if (shortLinkService.error()) {
        <div class="error-message">
          {{ shortLinkService.error() }}
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button 
        mat-raised-button 
        color="warn" 
        (click)="onConfirm()"
        [disabled]="shortLinkService.isDeleting()">
        Delete
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .link-info {
      background: #f5f5f5;
      padding: 16px;
      border-radius: 4px;
      margin: 16px 0;
      border-left: 4px solid #2196f3;
    }
    
    .warning-text {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #ff9800;
      background: #fff3e0;
      padding: 12px;
      border-radius: 4px;
      margin: 16px 0;
    }
    
    .error-message {
      color: #f44336;
      font-size: 14px;
      margin-top: 8px;
      padding: 8px;
      background: #ffebee;
      border-radius: 4px;
    }
    
    mat-dialog-content {
      max-height: 60vh;
      overflow-y: auto;
    }
  `]
})
export class DeleteLinkDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<DeleteLinkDialogComponent>);
  readonly shortLinkService = inject(ShortLinkService);
  readonly data: DeleteLinkDialogData = inject(MAT_DIALOG_DATA);

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.shortLinkService.delete(this.data.link.id).subscribe({
      next: () => {
        this.dialogRef.close(true);
      }
    });
  }
}
