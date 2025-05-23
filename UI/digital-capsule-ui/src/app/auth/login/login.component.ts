import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';


@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  hidePassword = true;
  username = '';
  password = '';
  
  constructor(private authService : AuthService, private router : Router){}

  onLogin(form: NgForm) {
  if (form.invalid) {
    return;
  }
  const { username, password } = form.value;
  this.authService.login({ username, password }).subscribe({
    next: (res) => {
      console.log('Login Successful:', res);
      // Ensure token is stored before navigating
      const token = localStorage.getItem('authToken');
      if (token) {
        this.router.navigate(['/dashboard']).then(() => {
          console.log('Navigated to dashboard');
        });
      } else {
        console.error('Token not found after login');
      }
    },
    error: (err) => {
      console.error('Login failed:', err);
    }
  });
}
}
