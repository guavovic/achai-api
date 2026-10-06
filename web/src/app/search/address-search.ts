import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { HttpResponse } from '@angular/common/http';
import { Observable, Subscription, finalize, timer } from 'rxjs';
import { AchaiApi, Address, BatchItem, Consensus } from '../core/api/achai-api';
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
  | { status: 'success'; url: string; response: ApiResponse; addresses: Address[]; note?: string }
  | { status: 'error'; url: string; response: ApiResponse | null; message: string };

export type SearchQuery =
  | { cep: string }
  | { uf: string; cidade: string; logradouro: string }
  | { ceps: string }
  | { consenso: string };

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
    const url = this.api.zipCodeUrl(zipCode.replace('-', ''));
    this.run<Address>(url, this.api.get(url), (address) => [address]);
  }

  byStreet(state: string, city: string, street: string): void {
    this.querySignal.set({ uf: state, cidade: city, logradouro: street });
    const url = this.api.streetUrl(state, city, street);
    this.run<Address[]>(url, this.api.get(url), (addresses) => addresses);
  }

  byConsensus(zipCode: string): void {
    const digits = zipCode.replace('-', '');
    this.querySignal.set({ consenso: digits });
    const url = this.api.consensusUrl(digits);
    this.run<Consensus>(
      url,
      this.api.get(url),
      (consensus) =>
        (consensus.fontes ?? []).flatMap((source) => (source.endereco ? [source.endereco] : [])),
      consensusNote,
    );
  }

  byZipCodes(zipCodes: string[]): void {
    this.querySignal.set({ ceps: zipCodes.join(',') });
    const url = this.api.batchUrl();
    this.run<BatchItem[]>(
      url,
      this.api.post(url, { ceps: zipCodes }),
      (items) => items.flatMap((item) => (item.endereco ? [item.endereco] : [])),
      batchNote,
    );
  }

  private run<T>(
    url: string,
    request: Observable<HttpResponse<T>>,
    toAddresses: (body: T) => Address[],
    note?: (body: T) => string,
  ): void {
    this.running?.unsubscribe();
    this.stateSignal.set({ status: 'loading', url, slow: false });

    const slowNotice = timer(SLOW_RESPONSE_MS).subscribe(() =>
      this.stateSignal.update((state) =>
        state.status === 'loading' ? { ...state, slow: true } : state,
      ),
    );
    const startedAt = performance.now();
    const elapsed = () => Math.round(performance.now() - startedAt);

    this.running = request.pipe(finalize(() => slowNotice.unsubscribe())).subscribe({
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
          ...(note ? { note: note(response.body as T) } : {}),
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

const BATCH_STATUS: Record<string, [string, string]> = {
  encontrado: ['encontrado', 'encontrados'],
  nao_encontrado: ['não encontrado', 'não encontrados'],
  invalido: ['inválido', 'inválidos'],
};

function batchNote(items: BatchItem[]): string {
  const counts = new Map<string, number>();
  for (const item of items) counts.set(item.status ?? '', (counts.get(item.status ?? '') ?? 0) + 1);

  const parts = [...counts].map(([status, count]) => {
    const [one, many] = BATCH_STATUS[status] ?? [status, status];
    return `${count} ${count === 1 ? one : many}`;
  });
  return `${items.length} CEPs: ${parts.join(', ')}`;
}

function consensusNote(consensus: Consensus): string {
  if (consensus.concordam) return 'as duas fontes concordam';
  const differences = consensus.divergencias ?? [];
  if (differences.includes('encontrado')) return 'só uma das fontes encontrou o CEP';
  if (differences.length === 0) return 'nenhuma fonte encontrou o CEP';
  return `as fontes divergem em: ${differences.join(', ')}`;
}
