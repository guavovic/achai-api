import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  output,
  signal,
} from '@angular/core';
import {
  LucideCheck,
  LucideCircleAlert,
  LucideCopy,
  LucideLoaderCircle,
  LucideMailbox,
  LucideMap,
  LucideMapPin,
  LucideSignpost,
} from '@lucide/angular';
import { Address } from '../../core/api/achai-api';
import { Logo } from '../../core/logo/logo';
import { AddressSearch } from '../address-search';
import { STREET_EXAMPLES, StreetExample, ZIP_CODE_EXAMPLES, pickRandom } from '../examples';

@Component({
  selector: 'app-search-results',
  imports: [
    Logo,
    LucideCheck,
    LucideCircleAlert,
    LucideCopy,
    LucideLoaderCircle,
    LucideMailbox,
    LucideMap,
    LucideMapPin,
    LucideSignpost,
  ],
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

  // Sorteados uma vez por visita, para cada pessoa ver exemplos de lugares diferentes.
  readonly examples = {
    zipCode: pickRandom(ZIP_CODE_EXAMPLES),
    street: pickRandom(STREET_EXAMPLES),
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
