import {
  ChangeDetectionStrategy,
  Component,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  linkedSignal,
  untracked,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AchaiApi } from '../core/api/achai-api';
import { RoutePath } from '../core/route-path/route-path';
import { AddressSearch, SearchQuery } from '../search/address-search';
import { BatchSearch } from '../search/batch/batch-search';
import { STREET_EXAMPLES, ZIP_CODE_EXAMPLES, pickRandom } from '../search/examples';
import { ResponsePanel } from '../search/response/response-panel';
import { RecentSearches, isBatchQuery, isZipCodeQuery } from '../search/recent-searches';
import { CodeSnippets } from '../search/snippets/code-snippets';
import { BatchBody } from '../search/snippets/snippets';
import { StreetSearch } from '../search/street/street-search';
import { maskZipCode } from '../search/zip-code/zip-code-mask';
import { ZipCodeSearch } from '../search/zip-code/zip-code-search';

type Tab = 'zipCode' | 'street' | 'batch';

@Component({
  selector: 'app-playground-page',
  imports: [ZipCodeSearch, StreetSearch, BatchSearch, ResponsePanel, CodeSnippets, RoutePath],
  templateUrl: './playground-page.html',
  styleUrl: './playground-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaygroundPage {
  private readonly search = inject(AddressSearch);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly recent = inject(RecentSearches);

  readonly endpoint = input<string>();
  readonly cep = input<string>();
  readonly uf = input<string>();
  readonly cidade = input<string>();
  readonly logradouro = input<string>();
  readonly ceps = input<string>();

  protected readonly tab = linkedSignal<Tab>(() => {
    if (this.endpoint() === 'lote' || this.ceps()) return 'batch';
    return this.endpoint() === 'logradouro' || this.logradouro() ? 'street' : 'zipCode';
  });
  protected readonly baseUrl = inject(AchaiApi).baseUrl;
  protected readonly examples = {
    zipCode: pickRandom(ZIP_CODE_EXAMPLES),
    street: pickRandom(STREET_EXAMPLES),
    batch: ZIP_CODE_EXAMPLES.slice(0, 3),
  };

  protected readonly requestUrl = computed(() => {
    const state = this.search.state();
    if (state.status !== 'idle') return state.url;

    const paths: Record<Tab, string> = {
      zipCode: '/buscar/{cep}',
      street: '/buscar/{uf}/{cidade}/{logradouro}',
      batch: '/buscar/lote',
    };
    return `${this.baseUrl}${paths[this.tab()]}`;
  });

  protected readonly requestBody = computed<BatchBody | undefined>(() => {
    if (!this.requestUrl().endsWith('/buscar/lote')) return undefined;
    const query = this.search.query();
    const ceps = query && isBatchQuery(query) ? query.ceps.split(',') : [...this.examples.batch];
    return { ceps };
  });

  private readonly zipCodeSearch = viewChild.required(ZipCodeSearch);
  private readonly streetSearch = viewChild.required(StreetSearch);
  private readonly batchSearch = viewChild.required(BatchSearch);

  constructor() {
    afterNextRender(() => this.searchFromUrl());

    effect(() => {
      const query = this.search.query();
      if (query)
        void this.router.navigate([], {
          relativeTo: this.route,
          queryParams: query,
          replaceUrl: true,
        });
    });

    effect(() => {
      if (this.search.state().status !== 'success') return;
      const query = untracked(this.search.query);
      if (query) this.recent.add(query);
    });
  }

  protected recentLabel(query: SearchQuery): string {
    if (isZipCodeQuery(query)) return maskZipCode(query.cep);
    if (isBatchQuery(query)) return `lote de ${query.ceps.split(',').length} CEPs`;
    return `${query.logradouro}, ${query.cidade}/${query.uf}`;
  }

  protected searchRecent(query: SearchQuery): void {
    if (isBatchQuery(query)) {
      this.tab.set('batch');
      this.batchSearch().searchFor(query.ceps.split(','));
    } else if (isZipCodeQuery(query)) {
      this.tab.set('zipCode');
      this.zipCodeSearch().searchFor(query.cep);
    } else {
      this.tab.set('street');
      this.streetSearch().searchFor(query.uf, query.cidade, query.logradouro);
    }
  }

  private searchFromUrl(): void {
    const [cep, uf, cidade, logradouro] = [this.cep(), this.uf(), this.cidade(), this.logradouro()];
    const ceps = this.ceps();

    if (ceps) this.batchSearch().searchFor(ceps.split(','));
    else if (cep) this.zipCodeSearch().searchFor(cep);
    else if (uf && cidade && logradouro) this.streetSearch().searchFor(uf, cidade, logradouro);
  }

  protected searchZipCodeExample(): void {
    this.zipCodeSearch().searchFor(this.examples.zipCode);
  }

  protected searchBatchExample(): void {
    this.batchSearch().searchFor([...this.examples.batch]);
  }

  protected searchStreetExample(): void {
    const { state, city, street } = this.examples.street;
    this.streetSearch().searchFor(state, city, street);
  }
}
