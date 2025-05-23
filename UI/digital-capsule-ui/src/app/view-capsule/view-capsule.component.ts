import { Component, OnInit, ViewChild, AfterViewInit, TemplateRef, ChangeDetectorRef } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { CapsuleService, Capsule, CapsulesResponse } from '../services/capsule.service';
import { AuthService } from '../services/auth.service';
import { Observable, of } from 'rxjs';
import { catchError, map, tap, timeout } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { jwtDecode } from 'jwt-decode';
import { FormControl, Validators } from '@angular/forms';
import { environment } from '../../environments/environment';
import { ImageDialogComponent } from '../image-dialog/image-dialog.component';
import moment from 'moment';

interface DisplayCapsule extends Capsule {}

@Component({
  selector: 'app-view-capsule',
  standalone: false,
  templateUrl: './view-capsule.component.html',
  styleUrls: ['./view-capsule.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0px', minHeight: '0', display: 'none' })),
      state('expanded', style({ height: '*', display: 'block' })),
      transition('expanded <=> collapsed', animate('300ms ease-in-out')),
    ]),
  ]
})
export class ViewCapsuleComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['title', 'createdAt', 'status', 'unlockDate', 'actions'];
  dataSource = new MatTableDataSource<DisplayCapsule>();
  expandedElement: DisplayCapsule | null = null;

  isLoadingResults = false;
  isRateLimitReached = false;
  errorMessage: string | null = null;
  resultsLength = 0;

  collaboratorFormVisible: { [id: number]: boolean } = {};
  contributionFormVisible: { [id: number]: boolean } = {};

  collaboratorEmail = new FormControl('', [Validators.required, Validators.email]);
  contributionText = '';
  selectedContributionFile?: File;

  currentUserId: string | null = null;
  sealDate: Date | null = null;
  today = new Date();
  currentCapsuleId: number | null = null;

  searchTerm: string = '';

  @ViewChild(MatSort, { static: false }) sort!: MatSort;
  @ViewChild(MatPaginator, { static: false }) paginator!: MatPaginator;
  @ViewChild('sealDialog') sealDialog!: TemplateRef<any>;

  constructor(
    private capsuleService: CapsuleService,
    private authService: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {
    this.today.setHours(0, 0, 0, 0);
  }

  ngOnInit() {
    this.getCurrentUserId().subscribe(userId => {
      this.currentUserId = userId;
      this.setFilterPredicate();
      this.loadCapsules();
    });
  }

  ngAfterViewInit() {
    this.tryInitializeTableComponents(0);
  }

  private tryInitializeTableComponents(attempt: number) {
    if (!this.sort || !this.paginator) {
      console.warn(`MatSort or MatPaginator not initialized (attempt ${attempt + 1})`);
      if (attempt < 3) {
        setTimeout(() => this.tryInitializeTableComponents(attempt + 1), 100 * (attempt + 1));
        return;
      } else {
        console.error('MatSort or MatPaginator failed to initialize after retries');
        return;
      }
    }

    this.initializeTableComponents();
  }

  private initializeTableComponents() {
    console.log('Initializing table components');
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;

    this.sort.active = 'unlockDate';
    this.sort.direction = 'desc';
    this.sort.sortChange.emit({ active: this.sort.active, direction: this.sort.direction });

    this.sort.sortChange.subscribe(() => {
      console.log('Sort changed:', { active: this.sort.active, direction: this.sort.direction });
      if (this.paginator) {
        this.paginator.pageIndex = 0;
      }
      this.loadCapsules();
    });

    this.paginator.page.subscribe(event => {
      console.log('Page event:', { pageIndex: event.pageIndex, pageSize: event.pageSize });
      this.loadCapsules();
    });

    this.loadCapsules();
    this.cdr.detectChanges();
  }

  onSearchChange() {
    this.dataSource.filter = this.searchTerm.trim().toLowerCase();
    if (this.paginator) {
      this.paginator.pageIndex = 0;
      console.log('Search term changed:', this.searchTerm);
      this.loadCapsules();
    } else {
      console.warn('Paginator not initialized during search');
      this.loadCapsules();
    }
  }

  loadCapsules() {
    this.isLoadingResults = true;
    this.errorMessage = null;
    this.isRateLimitReached = false;

    const sortBy = this.sort?.active || 'unlockDate';
    const sortDirection = this.sort?.direction || 'desc';
    const pageIndex = this.paginator ? Math.max(0, this.paginator.pageIndex) : 0;
    const pageSize = this.paginator ? Math.max(1, this.paginator.pageSize) : 10;

    console.log('Loading capsules with params:', { sortBy, sortDirection, pageIndex, pageSize, search: this.searchTerm });

    this.capsuleService.getUserCapsules(sortBy, sortDirection, pageIndex, pageSize, this.searchTerm || undefined).pipe(
      timeout(15000),
      tap((response: CapsulesResponse) => {
        console.log('API Response:', { capsules: response.capsules.length, totalCount: response.totalCount });
      }),
      map((response: CapsulesResponse) => {
        this.resultsLength = response.totalCount || response.capsules.length; // Fallback to capsules.length if totalCount is missing
        console.log('Setting resultsLength:', this.resultsLength);
        return response.capsules as DisplayCapsule[];
      }),
      catchError((error: HttpErrorResponse) => {
        this.isLoadingResults = false;
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          error: error.error
        });
        if (error.status === 429) {
          this.isRateLimitReached = true;
          this.errorMessage = 'API rate limit reached. Please try again later.';
        } else if (error.status === 401) {
          this.errorMessage = 'Session expired or unauthorized. Please log in again.';
          setTimeout(() => {
            this.authService.logout();
            window.location.href = '/login';
          }, 2000);
        } else if (error.status === 0) {
          this.errorMessage = 'Unable to connect to the backend. Please ensure the server is running.';
        } else {
          this.errorMessage = `Error fetching capsules: ${error.message}`;
        }
        this.resultsLength = 0; // Reset resultsLength on error
        return of([]);
      })
    ).subscribe({
      next: (capsules: DisplayCapsule[]) => {
        this.dataSource.data = capsules;
        this.isLoadingResults = false;
        console.log('DataSource updated with:', { capsules: capsules.length, resultsLength: this.resultsLength });
        //this.dataSource.filter = this.searchTerm.trim().toLowerCase();
        if (this.paginator) {
          this.paginator.length = this.resultsLength; // Explicitly update paginator length
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoadingResults = false;
        this.errorMessage = 'Unexpected error occurred. Please try again.';
        console.error('Unexpected error:', err);
        this.resultsLength = 0;
        this.cdr.detectChanges();
      }
    });
  }

  private setFilterPredicate() {
  this.dataSource.filterPredicate = (data: DisplayCapsule, filter: string) => {
    const searchTerm = filter.trim().toLowerCase();
    const title = data.title?.toLowerCase() || '';
    const description = data.description?.toLowerCase() || '';
    const status = data.status?.toLowerCase() || '';
    const unlockDate = data.unlockDate ? new Date(data.unlockDate).toLocaleDateString().toLowerCase() : '';

    return (
      title.includes(searchTerm) ||
      description.includes(searchTerm) ||
      status.includes(searchTerm) ||
      unlockDate.includes(searchTerm)
    );
  };
}

  getCurrentUserId(): Observable<string | null> {
    const token = localStorage.getItem('authToken');
    if (!token) {
      console.warn('No auth token found');
      return of(null);
    }
    try {
      const decoded: any = jwtDecode(token);
      const userId = decoded.sub || decoded.nameid || null;
      return of(userId);
    } catch (error) {
      console.error('Error decoding JWT:', error);
      return of(null);
    }
  }

  getChipColor(status: string): 'primary' | 'accent' | 'warn' {
    switch (status) {
      case 'sealed': return 'warn';
      case 'unsealed': return 'accent';
      case 'unlocked': return 'primary';
      default: return 'primary';
    }
  }

  toggleRow(row: DisplayCapsule): void {
    if (row.status === 'sealed') {
      this.expandedElement = null;
      return;
    }
    this.expandedElement = this.expandedElement === row ? null : row;
  }

  toggleCollaboratorForm(id: number) {
    this.collaboratorFormVisible[id] = !this.collaboratorFormVisible[id];
    if (!this.collaboratorFormVisible[id]) {
      this.collaboratorEmail.reset();
    }
  }

  toggleContributionForm(id: number) {
    this.contributionFormVisible[id] = !this.contributionFormVisible[id];
    if (!this.contributionFormVisible[id]) {
      this.contributionText = '';
      this.selectedContributionFile = undefined;
    }
  }

  onFileSelected(event: any): void {
    if (event.target.files.length > 0) {
      const file = event.target.files[0];
      const validImageTypes = ['image/jpeg', 'image/png', 'image/gif'];
      if (!validImageTypes.includes(file.type)) {
        this.snackBar.open('Please select a valid image (JPEG, PNG, or GIF)', 'Close', { duration: 3000 });
        return;
      }
      this.selectedContributionFile = file;
    }
  }

  addCollaborator(id: number) {
    if (this.collaboratorEmail.invalid) {
      this.snackBar.open('Please enter a valid email address', 'Close', { duration: 3000 });
      return;
    }

    const email = this.collaboratorEmail.value!.trim();

    this.capsuleService.addCollaborator(id, email).subscribe({
      next: (response: any) => {
        console.log('addCollaborator response:', response);
        const message = typeof response === 'string' ? response : response.message || `Added collaborator to capsule ${id}`;
        this.snackBar.open(message, 'Close', { duration: 3000 });
        this.collaboratorEmail.reset();
        this.toggleCollaboratorForm(id);
      },
      error: (error: HttpErrorResponse) => {
        const errorMessage = error.error?.Errors?.[0] || error.error || 'Error adding collaborator';
        this.snackBar.open(errorMessage, 'Close', { duration: 3000 });
      }
    });
  }

  openSealDialog(id: number) {
    this.currentCapsuleId = id;
    this.sealDate = null;
    this.dialog.open(this.sealDialog);
  }

  closeSealDialog() {
    this.dialog.closeAll();
    this.sealDate = null;
    this.currentCapsuleId = null;
  }

  confirmSeal() {
    if (!this.sealDate || !this.currentCapsuleId) {
      this.snackBar.open('Please select a valid date', 'Close', { duration: 3000 });
      return;
    }
    if (this.sealDate <= this.today) {
      this.snackBar.open('Unlock date must be in the future', 'Close', { duration: 3000 });
      return;
    }

    const localSealDate = moment(this.sealDate).startOf('day').format('YYYY-MM-DDTHH:mm:ss');

    this.capsuleService.sealCapsule(this.currentCapsuleId, localSealDate).subscribe({
      next: (response: string) => {
        this.snackBar.open(response || `Capsule ${this.currentCapsuleId} sealed successfully`, 'Close', { duration: 3000 });
        this.dialog.closeAll();
        this.sealDate = null;
        this.currentCapsuleId = null;
        this.loadCapsules();
      },
      error: (error: HttpErrorResponse) => {
        this.snackBar.open(`Error sealing capsule: ${error.error || error.message}`, 'Close', { duration: 3000 });
      }
    });
  }

  addContribution(capsuleId: number) {
    if (!this.currentUserId) {
      this.snackBar.open('User not authenticated', 'Close', { duration: 3000 });
      return;
    }

    if (!this.contributionText && !this.selectedContributionFile) {
      this.snackBar.open('Please provide either text or an image for the contribution', 'Close', { duration: 3000 });
      return;
    }

    const formData = new FormData();
    formData.append('capsuleId', capsuleId.toString());
    formData.append('contributorId', this.currentUserId);
    
    if (this.contributionText) {
      formData.append('TextContent', this.contributionText);
    }
    
    if (this.selectedContributionFile) {
      formData.append('image', this.selectedContributionFile);
    }

    this.capsuleService.addContribution(formData).subscribe({
      next: (response: string) => {
        console.log('Contribution Response:', response);
        const message = response || `Contribution added to capsule ${capsuleId}`;
        this.snackBar.open(message, 'Close', { duration: 3000 });
        this.contributionText = '';
        this.selectedContributionFile = undefined;
        this.toggleContributionForm(capsuleId);
        this.loadCapsules();
      },
      error: (error: HttpErrorResponse) => {
        console.error('addContribution error:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          error: error.error
        });
        const errorMessage = error.error?.message || error.error?.Errors?.[0] || error.message || 'Error adding contribution';
        this.snackBar.open(errorMessage, 'Close', { duration: 3000 });
      }
    });
  }

  getImageUrl(imagePath: string): string {
    if (!imagePath) return '';
    return `${environment.apiUrl}${imagePath}`;
  }

  openImageDialog(imageUrl: string): void {
    this.dialog.open(ImageDialogComponent, {
      data: { imageUrl: this.getImageUrl(imageUrl) },
      maxWidth: '95vw',
      maxHeight: '85vh',
      panelClass: 'image-dialog-panel'
    });
  }
}