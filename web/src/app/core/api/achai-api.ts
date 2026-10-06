import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { components } from './schema';

export type Address = components['schemas']['AddressResponse'];
export type City = components['schemas']['CityResponse'];
export type BatchItem = components['schemas']['BatchItem'];
export type Consensus = components['schemas']['ConsensusResponse'];
export type CoordinatesResponse = components['schemas']['CoordinatesResponse'];
export type Distance = components['schemas']['DistanceResponse'];
export type StatusResponse = components['schemas']['StatusResponse'];

@Injectable({ providedIn: 'root' })
export class AchaiApi {
  private readonly http = inject(HttpClient);

  readonly baseUrl = environment.apiBaseUrl;
  readonly openApiUrl = `${this.baseUrl}/openapi/v1.json`;
  readonly statusUrl = `${this.baseUrl}/status`;

  zipCodeUrl(zipCode: string): string {
    return `${this.baseUrl}/buscar/${encodeURIComponent(zipCode)}`;
  }

  streetUrl(state: string, city: string, street: string): string {
    const path = [state, city, street].map(encodeURIComponent).join('/');
    return `${this.baseUrl}/buscar/${path}`;
  }

  consensusUrl(zipCode: string): string {
    return `${this.baseUrl}/buscar/${encodeURIComponent(zipCode)}/consenso`;
  }

  coordinatesUrl(zipCode: string): string {
    return `${this.baseUrl}/buscar/${encodeURIComponent(zipCode.replace('-', ''))}/coordenadas`;
  }

  distanceUrl(origin: string, destination: string): string {
    const path = [origin, destination].map((zipCode) =>
      encodeURIComponent(zipCode.replace('-', '')),
    );
    return `${this.baseUrl}/distancia/${path.join('/')}`;
  }

  batchUrl(): string {
    return `${this.baseUrl}/buscar/lote`;
  }

  citiesUrl(state: string): string {
    return `${this.baseUrl}/buscar/cidades/${encodeURIComponent(state)}`;
  }

  get<T>(url: string): Observable<HttpResponse<T>> {
    return this.http.get<T>(url, { observe: 'response' });
  }

  post<T>(url: string, body: unknown): Observable<HttpResponse<T>> {
    return this.http.post<T>(url, body, { observe: 'response' });
  }

  wakeUp(): void {
    this.http
      .get(`${this.baseUrl}/health`, { responseType: 'text' })
      .subscribe({ error: () => {} });
  }
}
