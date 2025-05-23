import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

interface LoginRequest {
  username: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authUrl = 'https://localhost:7163/api/Auth';
  private loggedIn = new BehaviorSubject<boolean>(false); 
  constructor(private http: HttpClient) {
    const token = localStorage.getItem('authToken');
    this.loggedIn.next(!!token); 
  }

  isLoggedIn(): Observable<boolean> {
    return this.loggedIn.asObservable();
  }


  isLoggedInSync(): boolean {
    return this.loggedIn.getValue();
  }

  login(credentials: LoginRequest): Observable<any> {
    return this.http.post(`${this.authUrl}/login`, credentials).pipe(
      tap((response: any) => {
    
        if (response && response.token) {
          console.log('Storing token:', response.token);
          localStorage.setItem('authToken', response.token); 
          this.loggedIn.next(true); 
        }
      })
    );
  }

  register(data: { username: string; email: string; password: string }): Observable<any> {
    return this.http.post(`${this.authUrl}/register`, data);
  }

  logout(): void {
    localStorage.removeItem('authToken'); 
    this.loggedIn.next(false); 
  }
}