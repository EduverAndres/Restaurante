import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CustomerAddress {
  id: string;
  label: string;
  address: string;
  latitude?: number | null;
  longitude?: number | null;
  isDefault: boolean;
}

export interface CreateAddressRequest {
  label: string;
  address: string;
  latitude?: number | null;
  longitude?: number | null;
  isDefault?: boolean;
}

@Injectable({ providedIn: 'root' })
export class AddressService {
  private readonly apiUrl = `${environment.apiUrl}/customer/addresses`;

  constructor(private http: HttpClient) {}

  getAddresses(): Observable<CustomerAddress[]> {
    return this.http.get<any>(this.apiUrl).pipe(
      map((res: any) => res.data || res)
    );
  }

  createAddress(data: CreateAddressRequest): Observable<CustomerAddress> {
    return this.http.post<any>(this.apiUrl, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  updateAddress(id: string, data: CreateAddressRequest): Observable<CustomerAddress> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data).pipe(
      map((res: any) => res.data || res)
    );
  }

  deleteAddress(id: string): Observable<boolean> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`).pipe(
      map((res: any) => (res.data !== undefined ? res.data : res))
    );
  }
}
