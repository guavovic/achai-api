import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  output,
  signal,
} from '@angular/core';
import { Address } from '../../core/api/achai-api';
import { AddressSearch } from '../address-search';

export interface StreetExample {
  state: string;
  city: string;
  street: string;
}

@Component({
  selector: 'app-search-results',
  templateUrl: './search-results.html',
  styleUrl: './search-results.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchResults {
  private readonly search = inject(AddressSearch);

  readonly state = this.search.state;
  readonly copied = signal<string | null>(null);

  readonly addresses = computed(() => {
    const state = this.state();
    return state.status === 'success' ? state.addresses : [];
  });
  readonly slow = computed(() => {
    const state = this.state();
    return state.status === 'loading' && state.slow;
  });
  readonly errorMessage = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });

  readonly zipCodeExample = output<string>();
  readonly streetExample = output<StreetExample>();

  readonly examples = {
    zipCode: '01001-000',
    street: { state: 'RJ', city: 'Rio de Janeiro', street: 'Atlântica' },
  };

  title(address: Address): string {
    return address.logradouro || 'CEP geral da cidade';
  }

  details(address: Address): string {
    return [address.complemento, address.bairro].filter(Boolean).join(' · ');
  }

  place(address: Address): string {
    const city = [address.localidade, address.uf].filter(Boolean).join('/');
    return [city, address.regiao].filter(Boolean).join(' · ');
  }

  async copy(address: Address): Promise<void> {
    const text = [
      address.logradouro,
      address.bairro,
      `${address.localidade}/${address.uf}`,
      `CEP ${address.cep}`,
    ]
      .filter(Boolean)
      .join(', ');

    try {
      await navigator.clipboard.writeText(text);
      this.copied.set(address.cep);
      setTimeout(() => this.copied.set(null), 2000);
    } catch {
      // Sem permissão para a área de transferência: o endereço continua na tela para copiar à mão.
    }
  }
}
