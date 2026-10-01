import { Injectable, inject, signal } from '@angular/core';
import { Observable, Subscription, finalize, map, timer } from 'rxjs';
import { AchaiApi, Address } from '../core/api/achai-api';
import { problemMessage } from '../core/api/problem-message';

export const SLOW_RESPONSE_MS = 3000;

export type SearchState =
  | { status: 'idle' }
  | { status: 'loading'; slow: boolean }
  | { status: 'success'; addresses: Address[] }
  | { status: 'error'; message: string };

@Injectable({ providedIn: 'root' })
export class AddressSearch {
  private readonly api = inject(AchaiApi);
  private readonly stateSignal = signal<SearchState>({ status: 'idle' });
  private running?: Subscription;

  readonly state = this.stateSignal.asReadonly();

  byZipCode(zipCode: string): void {
    this.run(
      this.api.getAddressByZipCode(zipCode.replace('-', '')).pipe(map((address) => [address])),
    );
  }

  byStreet(state: string, city: string, street: string): void {
    this.run(this.api.searchByStreet(state, city, street));
  }

  private run(request: Observable<Address[]>): void {
    this.running?.unsubscribe();
    this.stateSignal.set({ status: 'loading', slow: false });

    const slowNotice = timer(SLOW_RESPONSE_MS).subscribe(() =>
      this.stateSignal.update((state) =>
        state.status === 'loading' ? { ...state, slow: true } : state,
      ),
    );

    this.running = request.pipe(finalize(() => slowNotice.unsubscribe())).subscribe({
      next: (addresses) => this.stateSignal.set({ status: 'success', addresses }),
      error: (error) => this.stateSignal.set({ status: 'error', message: problemMessage(error) }),
    });
  }
}
