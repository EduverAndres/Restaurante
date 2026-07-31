import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, throwError, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface User {
  id: string;
  email: string;
  name: string;
  phone: string;
  role: 'customer' | 'restaurant';
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'restaurante_token';
  private readonly refreshKey = 'restaurante_refresh';
  private readonly userKey = 'restaurante_user';

  isLoggedIn = signal(false);
  currentUser = signal<User | null>(null);
  userRole = signal<'customer' | 'restaurant' | null>(null);

  constructor(private http: HttpClient, private router: Router) {
    this.loadStoredUser();
  }

  private loadStoredUser(): void {
    const token = localStorage.getItem(this.tokenKey);
    const user = localStorage.getItem(this.userKey);
    if (token && user) {
      try {
        const parsed = JSON.parse(user) as User;
        this.currentUser.set(parsed);
        this.userRole.set(parsed.role);
        this.isLoggedIn.set(true);
      } catch {
        this.clearStorage();
      }
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap(res => this.handleAuthResponse(res)),
    );
  }

  register(name: string, email: string, password: string, phone: string, role: 'customer' | 'restaurant'): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { name, email, password, phone, role }).pipe(
      tap(res => this.handleAuthResponse(res)),
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refresh = localStorage.getItem(this.refreshKey);
    if (!refresh) return throwError(() => new Error('No refresh token'));
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken: refresh }).pipe(
      tap(res => this.handleAuthResponse(res)),
      catchError(err => {
        this.logout();
        return throwError(() => err);
      }),
    );
  }

  logout(): void {
    this.clearStorage();
    this.currentUser.set(null);
    this.userRole.set(null);
    this.isLoggedIn.set(false);
    this.router.navigate(['/']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private handleAuthResponse(res: AuthResponse): void {
    localStorage.setItem(this.tokenKey, res.token);
    localStorage.setItem(this.refreshKey, res.refreshToken);
    localStorage.setItem(this.userKey, JSON.stringify(res.user));
    this.currentUser.set(res.user);
    this.userRole.set(res.user.role);
    this.isLoggedIn.set(true);
  }

  private clearStorage(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshKey);
    localStorage.removeItem(this.userKey);
  }
}
