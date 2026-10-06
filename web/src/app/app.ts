import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { AchaiApi } from './core/api/achai-api';
import { Logo } from './core/logo/logo';
import { Theme } from './core/theme/theme';
import { AddressSearch } from './search/address-search';
import { STREET_EXAMPLES, ZIP_CODE_EXAMPLES, pickRandom } from './search/examples';
import { ResponsePanel } from './search/response/response-panel';
import { CodeSnippets } from './search/snippets/code-snippets';
import { StreetSearch } from './search/street/street-search';
import { ZipCodeSearch } from './search/zip-code/zip-code-search';

type Tab = 'zipCode' | 'street';

@Component({
  selector: 'app-root',
  imports: [Logo, ZipCodeSearch, StreetSearch, ResponsePanel, CodeSnippets],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly api = inject(AchaiApi);
  private readonly search = inject(AddressSearch);

  protected readonly theme = inject(Theme);
  protected readonly tab = signal<Tab>('zipCode');
  protected readonly baseUrl = this.api.baseUrl;
  protected readonly docsUrl = `${this.baseUrl}/docs`;
  protected readonly examples = {
    zipCode: pickRandom(ZIP_CODE_EXAMPLES),
    street: pickRandom(STREET_EXAMPLES),
  };

  protected readonly requestUrl = computed(() => {
    const state = this.search.state();
    if (state.status !== 'idle') return state.url;

    return this.tab() === 'zipCode'
      ? `${this.baseUrl}/buscar/{cep}`
      : `${this.baseUrl}/buscar/{uf}/{cidade}/{logradouro}`;
  });

  private readonly zipCodeSearch = viewChild.required(ZipCodeSearch);
  private readonly streetSearch = viewChild.required(StreetSearch);

  constructor() {
    this.api.wakeUp();
  }

  protected themeAction(): string {
    return this.theme.mode() === 'dark' ? 'Mudar para o tema claro' : 'Mudar para o tema escuro';
  }

  protected searchZipCodeExample(): void {
    this.zipCodeSearch().searchFor(this.examples.zipCode);
  }

  protected searchStreetExample(): void {
    const { state, city, street } = this.examples.street;
    this.streetSearch().searchFor(state, city, street);
  }
}
