import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Capsule {
  id: number;
  title: string;
  description: string;
  content: string;
  imageUrl?: string;
  image: string;
  unlockDate?: Date;
  creatorId: string;
  status: 'sealed' | 'unlocked' | 'unsealed';
  contributions?: ContributionViewDto[];
  createdAt?: Date;
}

export interface ContributionViewDto {
  textContent: string;
  imageUrl: string;
  createdAt: Date;
  collaboratorEmail: string; 
}

export interface CapsulesResponse {
  capsules: Capsule[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class CapsuleService {
  private apiUrl = 'https://localhost:7163/api/Capsule';
  private envUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('authToken');
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    return headers;
  }

  createCapsule(formData: FormData): Observable<string> {
    const headers = this.getAuthHeaders();
    return this.http.post<string>(`${this.apiUrl}/create`, formData, { headers });
  }

  getUserCapsules(sortBy: string, sortDirection: string, pageIndex: number, pageSize: number, search?: string): Observable<CapsulesResponse> {
    let params = new HttpParams()
      .set('sortBy', sortBy)
      .set('sortDirection', sortDirection)
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());
    
    if (search) {
      params = params.set('search', search);
    }

    const headers = this.getAuthHeaders();
    return this.http.get<CapsulesResponse>(`${this.apiUrl}/list`, { headers, params });
  }

  addCollaborator(capsuleId: number, email: string): Observable<string> {
    const headers = this.getAuthHeaders();
    return this.http.post<string>(`${this.apiUrl}/inviteCollaborator`, { CapsuleId: capsuleId, CollaboratorEmail: email }, { headers });
  }

  addContribution(formData: FormData): Observable<string> {
    const headers = this.getAuthHeaders();
    return this.http.post<string>(`${this.apiUrl}/contribute`, formData, { headers });
  }

  sealCapsule(capsuleId: number, unlockDate: string): Observable<string> {
    const headers = this.getAuthHeaders();
    return this.http.post<string>(`${this.apiUrl}/seal`, { capsuleId: capsuleId, unlockDate: unlockDate }, { headers });
  }
}