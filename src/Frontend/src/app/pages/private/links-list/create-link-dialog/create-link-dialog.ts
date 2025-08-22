import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CommonModule } from '@angular/common';
import { ShortLinkService } from '../../../../services/short-link.service';

@Component({
  selector: 'trq-create-link-dialog',
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
    <h2 mat-dialog-title>Create New Short Link</h2>
    
    <mat-dialog-content>
      <form [formGroup]="createForm" class="create-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Target URL</mat-label>
          <input matInput formControlName="targetUrl" type="url" placeholder="https://example.com">
          <mat-hint>The URL you want to shorten</mat-hint>
          @if (createForm.get('targetUrl')?.hasError('required')) {
            <mat-error>URL is required</mat-error>
          }
          @if (createForm.get('targetUrl')?.hasError('url')) {
            <mat-error>Please enter a valid URL</mat-error>
          }
        </mat-form-field>
      </form>
      
      @if (shortLinkService.isCreating()) {
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
        (click)="onCreate()"
        [disabled]="createForm.invalid || shortLinkService.isCreating()">
        Create
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .create-form {
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
      .create-form {
        min-width: 280px;
      }
    }
  `]
})
export class CreateLinkDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<CreateLinkDialogComponent>);
  readonly shortLinkService = inject(ShortLinkService);
  
  createForm: FormGroup;

  constructor() {
    this.createForm = this.fb.group({
      targetUrl: ['', [
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

  onCreate(): void {
    if (this.createForm.invalid) return;
    
    const formValue = this.createForm.value;
    this.shortLinkService.create(formValue.targetUrl).subscribe({
      next: (createdLink) => {
        this.dialogRef.close(createdLink);
      }
    });
  }
}
