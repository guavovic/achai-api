import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { components } from './schema';

// Os tipos vêm do documento OpenAPI da API (npm run api:types).
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

  /** Acorda a API logo que a página abre, enquanto a pessoa ainda está digitando. */
  wakeUp(): void {
    this.http
      .get(`${this.baseUrl}/health`, { responseType: 'text' })
      .subscribe({ error: () => {} });
  }
}
