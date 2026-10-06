import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  viewChild,
} from '@angular/core';
import { AchaiApi } from '../core/api/achai-api';
import { AddressSearch } from '../search/address-search';
import { STREET_EXAMPLES, ZIP_CODE_EXAMPLES, pickRandom } from '../search/examples';
import { ResponsePanel } from '../search/response/response-panel';
import { CodeSnippets } from '../search/snippets/code-snippets';
import { StreetSearch } from '../search/street/street-search';
import { ZipCodeSearch } from '../search/zip-code/zip-code-search';

type Tab = 'zipCode' | 'street';

@Component({
  selector: 'app-playground-page',
  imports: [ZipCodeSearch, StreetSearch, ResponsePanel, CodeSnippets],
  templateUrl: './playground-page.html',
  styleUrl: './playground-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaygroundPage {
  private readonly search = inject(AddressSearch);

  readonly endpoint = input<string>();

  protected readonly tab = linkedSignal<Tab>(() =>
    this.endpoint() === 'logradouro' ? 'street' : 'zipCode',
  );
  protected readonly baseUrl = inject(AchaiApi).baseUrl;
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

  protected searchZipCodeExample(): void {
    this.zipCodeSearch().searchFor(this.examples.zipCode);
  }

  protected searchStreetExample(): void {
    const { state, city, street } = this.examples.street;
    this.streetSearch().searchFor(state, city, street);
  }
}
