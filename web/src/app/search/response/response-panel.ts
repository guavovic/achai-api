import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
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
  readonly copied = signal<'json' | 'link' | null>(null);

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

    const { addresses, response, note } = state;
    if (note) return note;
    const searchedAs = response.searchedAs ? `, buscado como "${response.searchedAs}"` : '';
    if (addresses.length === 0)
      return 'nenhum endereço encontrado. confira o nome da rua e da cidade';
    if (addresses.length > 1) return `${addresses.length} endereços${searchedAs}`;
    return `${addresses[0].enderecoFormatado}${searchedAs}`;
  });

  reason(status: number): string {
    return reasonPhrase(status);
  }

  async copyJson(): Promise<void> {
    const response = this.response();
    if (response) await this.copy('json', JSON.stringify(response.body, null, 2));
  }

  async copyLink(): Promise<void> {
    await this.copy('link', location.href);
  }

  private async copy(what: 'json' | 'link', text: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(text);
      this.copied.set(what);
      setTimeout(() => this.copied.set(null), 2000);
    } catch {}
  }
}
