import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { FormsModule } from '@angular/forms';
import { PrivateToolbar } from '../../../shared/components/private-toolbar/private-toolbar';
import { ShortLinkService, ShortLinkDetailsDto } from '../../../services/short-link.service';
import { EditLinkDialogComponent } from './edit-link-dialog/edit-link-dialog';
import { DeleteLinkDialogComponent } from './delete-link-dialog/delete-link-dialog';
import { CreateLinkDialogComponent } from './create-link-dialog/create-link-dialog';

@Component({
  selector: 'trq-links-list',
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule,
    MatMenuModule,
    MatFormFieldModule,
    MatInputModule,
    MatDividerModule,
    PrivateToolbar
  ],
  templateUrl: './links-list.html',
  styleUrl: './links-list.scss'
})
export class LinksList implements OnInit {
  private readonly shortLinkService = inject(ShortLinkService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly searchTerm = signal('');
  readonly isLoading = computed(() => this.shortLinkService.isLoading());
  readonly error = computed(() => this.shortLinkService.error());
  
  readonly isMobile = signal(false);
  readonly isTablet = signal(false);
  
  readonly allLinks = computed(() => this.shortLinkService.shortLinks());
  readonly filteredLinks = computed(() => {
    const links = this.allLinks();
    const search = this.searchTerm().toLowerCase();
    
    if (!search) return links;
    
    return links.filter(link => 
      link.shortCode.toLowerCase().includes(search) ||
      link.targetUrl.toLowerCase().includes(search)
    );
  });

  readonly displayedColumns = computed(() => {
    if (this.isMobile()) {
      return ['shortCode', 'actions'];
    } else if (this.isTablet()) {
      return ['shortCode', 'targetUrl', 'actions'];
    } else {
      return ['shortCode', 'targetUrl', 'hits', 'createdAt', 'actions'];
    }
  });

  ngOnInit(): void {
    // Monitor breakpoints
    this.breakpointObserver.observe([
      Breakpoints.HandsetPortrait,
      Breakpoints.HandsetLandscape
    ]).subscribe(result => {
      this.isMobile.set(result.matches);
    });

    this.breakpointObserver.observe([
      Breakpoints.TabletPortrait,
      Breakpoints.TabletLandscape
    ]).subscribe(result => {
      this.isTablet.set(result.matches);
    });

    // Load links
    this.loadLinks();
  }

  loadLinks(): void {
    this.shortLinkService.getAll().subscribe();
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
  }

  copyToClipboard(link: ShortLinkDetailsDto): void {
    const url = this.shortLinkService.buildShortUrl(link.shortCode, link.shortUrl);
    navigator.clipboard.writeText(url).then(() => {
      this.snackBar.open('Link copied to clipboard!', 'Close', { duration: 2000 });
    });
  }

  createLink(): void {
    const dialogRef = this.dialog.open(CreateLinkDialogComponent, {
      width: this.isMobile() ? '95vw' : '500px',
      maxWidth: '500px',
      maxHeight: '90vh'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Link created successfully!', 'Close', { duration: 3000 });
      }
    });
  }

  editLink(link: ShortLinkDetailsDto): void {
    const dialogRef = this.dialog.open(EditLinkDialogComponent, {
      data: { link },
      width: this.isMobile() ? '95vw' : '500px',
      maxWidth: '500px',
      maxHeight: '90vh'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Link updated successfully!', 'Close', { duration: 3000 });
      }
    });
  }

  deleteLink(link: ShortLinkDetailsDto): void {
    const dialogRef = this.dialog.open(DeleteLinkDialogComponent, {
      data: { link },
      width: this.isMobile() ? '95vw' : '400px',
      maxWidth: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Link deleted successfully!', 'Close', { duration: 3000 });
      }
    });
  }

  openUrl(url: string): void {
    window.open(url, '_blank');
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getShortUrl(link: ShortLinkDetailsDto): string {
    return this.shortLinkService.buildShortUrl(link.shortCode, link.shortUrl);
  }
}
