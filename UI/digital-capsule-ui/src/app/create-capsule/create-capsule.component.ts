import { Component } from '@angular/core';
import { CapsuleService } from '../services/capsule.service';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-create-capsule',
  standalone: false,
  templateUrl: './create-capsule.component.html',
  styleUrls: ['./create-capsule.component.css']
})
export class CreateCapsuleComponent {
  capsule = {
    title: '',
    description: '',
    content: ''
  };

  selectedFile: File | null = null;
  unlockDate: Date | null = null;

  constructor(private capsuleService: CapsuleService, private router: Router) {}

  onFileSelected(event: any) {
    if (event.target.files.length > 0) {
      this.selectedFile = event.target.files[0];
    }
  }

  onSubmit(form: NgForm) {
    if (form.invalid) {
      alert('Please fill out all required fields.');
      return;
    }

    const formData = new FormData();
    formData.append('title', this.capsule.title);
    formData.append('description', this.capsule.description);
    formData.append('content', this.capsule.content || '');

    if (this.selectedFile) {
      formData.append('image', this.selectedFile);
    }

    // Note: unlockDate is not expected by the backend's CreateCapsule action, so it's omitted
    // If you want to include unlockDate, the backend DTO and action need to be updated

    this.capsuleService.createCapsule(formData).subscribe({
      next: (res: string) => {
        console.log('Success:', res);
        alert('Capsule created successfully!');
        this.router.navigate(['/view-capsules']);
      },
      error: (err: any) => {
        console.error('Capsule creation error:', err);
        alert('Failed to create capsule: ' + (err.error?.message || err.message));
      }
    });
  }
}