import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { components } from './schema';

export type Address = components['schemas']['AddressResponse'];
export type City = components['schemas']['CityResponse'];

@Injectable({ providedIn: 'root' })
export class AchaiApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getAddressByZipCode(zipCode: string): Observable<Address> {
    return this.http.get<Address>(`${this.baseUrl}/buscar/${encodeURIComponent(zipCode)}`);
  }

  searchByStreet(state: string, city: string, street: string): Observable<Address[]> {
    const path = [state, city, street].map(encodeURIComponent).join('/');
    return this.http.get<Address[]>(`${this.baseUrl}/buscar/${path}`);
  }

  citiesUrl(state: string): string {
    return `${this.baseUrl}/buscar/cidades/${encodeURIComponent(state)}`;
  }

  wakeUp(): void {
    this.http
      .get(`${this.baseUrl}/health`, { responseType: 'text' })
      .subscribe({ error: () => {} });
  }
}
