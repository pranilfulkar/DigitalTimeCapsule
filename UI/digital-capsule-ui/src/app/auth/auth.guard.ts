import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private router: Router) {}

  canActivate(): boolean {
  const token = localStorage.getItem('authToken');
  console.log('AuthGuard: Token found:', !!token);
  if (token && !this.isTokenExpired(token)) {
    console.log('AuthGuard: Token is valid');
    return true;
  }
  console.log('AuthGuard: Token missing or expired, redirecting to login');
  localStorage.removeItem('authToken');
  this.router.navigate(['/login']);
  return false;
}

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('Decoded token payload:', payload);
      const expiry = payload.exp;

      if (!expiry) return true;

      const now = Math.floor(Date.now() / 1000);
      return expiry < now;
    } catch (e) {
      return true;
    }
  }
}
