import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone:false,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {
  constructor(private router: Router) {}

  navigateToCreateCapsule() {
    this.router.navigate(['/create-capsule']);
  }

  navigateToViewCapsules() {
    this.router.navigate(['/view-capsules']);
  }
}