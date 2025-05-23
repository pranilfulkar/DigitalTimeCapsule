import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service'; // Import AuthService
import { jwtDecode } from 'jwt-decode';

interface DecodedToken {
  username?: string;
  email?: string;
  exp?: number;
}

@Component({
  selector: 'app-root',
  standalone:false,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  username = 'Guest';
  isLoggedIn = false;

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit() {
    this.authService.isLoggedIn().subscribe((loggedIn) => {
      this.isLoggedIn = loggedIn;
      if (loggedIn) {
        const token = localStorage.getItem('authToken');
        if (token) {
          try {
            const decoded: DecodedToken = jwtDecode(token);
            this.username = decoded.username || decoded.email || 'User';
          } catch (err) {
            console.error('Token decoding failed', err);
            this.username = 'User';
            this.isLoggedIn = false;
            this.authService.logout(); 
          }
        }
      } else {
        this.username = 'User';
      }
    });
  }

  logout() {
    this.authService.logout(); 
    this.router.navigate(['/login']);
  }
}