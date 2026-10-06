import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Address } from '../../core/api/achai-api';
import { reasonPhrase } from '../../core/api/reason-phrase';
import { AddressSearch } from '../address-search';
import { jsonTokens } from './json-tokens';

@Component({
  selector: 'app-response-panel',
  templateUrl: './response-panel.html',
  styleUrl: './response-panel.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResponsePanel {
  private readonly search = inject(AddressSearch);

  readonly state = this.search.state;
  readonly copied = signal(false);

  readonly response = computed(() => {
    const state = this.state();
    return state.status === 'success' || state.status === 'error' ? state.response : null;
  });
  readonly tokens = computed(() => {
    const response = this.response();
    return response ? jsonTokens(response.body) : [];
  });
  readonly slow = computed(() => {
    const state = this.state();
    return state.status === 'loading' && state.slow;
  });
  readonly errorMessage = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });
  readonly summary = computed(() => {
    const state = this.state();
    if (state.status !== 'success') return '';

    const { addresses } = state;
    if (addresses.length === 0)
      return 'nenhum endereço encontrado. confira o nome da rua e da cidade';
    if (addresses.length > 1) return `${addresses.length} endereços`;
    return oneLine(addresses[0]);
  });

  reason(status: number): string {
    return reasonPhrase(status);
  }

  async copyJson(): Promise<void> {
    const response = this.response();
    if (!response) return;

    try {
      await navigator.clipboard.writeText(JSON.stringify(response.body, null, 2));
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {}
  }
}

function oneLine(address: Address): string {
  return [
    address.logradouro || 'CEP geral da cidade',
    address.bairro,
    `${address.localidade}/${address.uf}`,
  ]
    .filter(Boolean)
    .join(', ');
}
