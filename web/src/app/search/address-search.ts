import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Subscription, finalize, timer } from 'rxjs';
import { AchaiApi, Address } from '../core/api/achai-api';
import { problemMessage } from '../core/api/problem-message';

export const SLOW_RESPONSE_MS = 3000;
export const SEARCHED_STREET_HEADER = 'X-Logradouro-Buscado';

export interface ApiResponse {
  status: number;
  ms: number;
  body: unknown;
  searchedAs?: string;
}

export type SearchState =
  | { status: 'idle' }
  | { status: 'loading'; url: string; slow: boolean }
  | { status: 'success'; url: string; response: ApiResponse; addresses: Address[] }
  | { status: 'error'; url: string; response: ApiResponse | null; message: string };

export type SearchQuery = { cep: string } | { uf: string; cidade: string; logradouro: string };

@Injectable({ providedIn: 'root' })
export class AddressSearch {
  private readonly api = inject(AchaiApi);
  private readonly stateSignal = signal<SearchState>({ status: 'idle' });
  private readonly querySignal = signal<SearchQuery | null>(null);
  private running?: Subscription;

  readonly state = this.stateSignal.asReadonly();
  readonly query = this.querySignal.asReadonly();

  byZipCode(zipCode: string): void {
    this.querySignal.set({ cep: zipCode.replace('-', '') });
    this.run<Address>(this.api.zipCodeUrl(zipCode.replace('-', '')), (address) => [address]);
  }

  byStreet(state: string, city: string, street: string): void {
    this.querySignal.set({ uf: state, cidade: city, logradouro: street });
    this.run<Address[]>(this.api.streetUrl(state, city, street), (addresses) => addresses);
  }

  private run<T>(url: string, toAddresses: (body: T) => Address[]): void {
    this.running?.unsubscribe();
    this.stateSignal.set({ status: 'loading', url, slow: false });

    const slowNotice = timer(SLOW_RESPONSE_MS).subscribe(() =>
      this.stateSignal.update((state) =>
        state.status === 'loading' ? { ...state, slow: true } : state,
      ),
    );
    const startedAt = performance.now();
    const elapsed = () => Math.round(performance.now() - startedAt);

    this.running = this.api
      .get<T>(url)
      .pipe(finalize(() => slowNotice.unsubscribe()))
      .subscribe({
        next: (response) =>
          this.stateSignal.set({
            status: 'success',
            url,
            response: {
              status: response.status,
              ms: elapsed(),
              body: response.body,
              ...searchedAs(response.headers.get(SEARCHED_STREET_HEADER)),
            },
            addresses: toAddresses(response.body as T),
          }),
        error: (error) =>
          this.stateSignal.set({
            status: 'error',
            url,
            message: problemMessage(error),
            response:
              error instanceof HttpErrorResponse && error.status !== 0
                ? { status: error.status, ms: elapsed(), body: error.error }
                : null,
          }),
      });
  }
}

function searchedAs(header: string | null): Pick<ApiResponse, 'searchedAs'> {
  return header ? { searchedAs: decodeURIComponent(header) } : {};
}
