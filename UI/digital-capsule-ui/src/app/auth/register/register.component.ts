import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-register',
  standalone:false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  username = '';
  email = '';
  password = '';
  hidePassword = true;

  constructor(private authService: AuthService, private snackBar: MatSnackBar) {}

  onRegister(form: NgForm) {
    if (form.invalid) return;

    const data = {
      username: this.username,
      email: this.email,
      password: this.password
    };

    this.authService.register(data).subscribe({
      next: (res) => {
        this.snackBar.open('Registration successful!', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.log('Registration successful:', res.message);
      },
      error: (err) => {
        this.snackBar.open('Registration failed. Please try again.', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.error('Registration failed:', err);
      }
    });
  }
}
