import { Component, inject, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CommonModule } from '@angular/common';
import { ShortLinkDetailsDto, ShortLinkService } from '../../../../services/short-link.service';

export interface EditLinkDialogData {
  link: ShortLinkDetailsDto;
}

@Component({
  selector: 'trq-edit-link-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressBarModule
  ],
  template: `
    <h2 mat-dialog-title>Edit Short Link</h2>
    
    <mat-dialog-content>
      <form [formGroup]="editForm" class="edit-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Short Code</mat-label>
          <input matInput formControlName="shortCode" placeholder="e.g., abc123">
          <mat-hint>4-12 alphanumeric characters</mat-hint>
          @if (editForm.get('shortCode')?.hasError('required')) {
            <mat-error>Short code is required</mat-error>
          }
          @if (editForm.get('shortCode')?.hasError('minlength') || editForm.get('shortCode')?.hasError('maxlength')) {
            <mat-error>Short code must be 4-12 characters long</mat-error>
          }
          @if (editForm.get('shortCode')?.hasError('pattern')) {
            <mat-error>Short code must contain only letters and numbers</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Target URL</mat-label>
          <input matInput formControlName="targetUrl" type="url" placeholder="https://example.com">
          @if (editForm.get('targetUrl')?.hasError('required')) {
            <mat-error>URL is required</mat-error>
          }
          @if (editForm.get('targetUrl')?.hasError('url')) {
            <mat-error>Please enter a valid URL</mat-error>
          }
        </mat-form-field>
      </form>
      
      @if (shortLinkService.isUpdating()) {
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
        color="primary" 
        (click)="onSave()"
        [disabled]="editForm.invalid || shortLinkService.isUpdating()">
        Save
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .edit-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
      min-width: 400px;
      padding: 16px 0;
    }
    
    .full-width {
      width: 100%;
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
    
    @media (max-width: 600px) {
      .edit-form {
        min-width: 280px;
      }
    }
  `]
})
export class EditLinkDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<EditLinkDialogComponent>);
  readonly shortLinkService = inject(ShortLinkService);
  readonly data: EditLinkDialogData = inject(MAT_DIALOG_DATA);
  
  editForm: FormGroup;

  constructor() {
    this.editForm = this.fb.group({
      shortCode: [this.data.link.shortCode, [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(12),
        Validators.pattern(/^[a-zA-Z0-9]+$/)
      ]],
      targetUrl: [this.data.link.targetUrl, [
        Validators.required,
        this.urlValidator
      ]]
    });
  }

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

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.editForm.invalid) return;
    
    const formValue = this.editForm.value;
    this.shortLinkService.update(
      this.data.link.id,
      formValue.targetUrl,
      formValue.shortCode
    ).subscribe({
      next: (updatedLink) => {
        this.dialogRef.close(updatedLink);
      }
    });
  }
}
