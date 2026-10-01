import { ChangeDetectionStrategy, Component, inject, signal, viewChild } from '@angular/core';
import { environment } from '../environments/environment';
import { AchaiApi } from './core/api/achai-api';
import { Theme } from './core/theme/theme';
import { SearchResults, StreetExample } from './search/results/search-results';
import { StreetSearch } from './search/street/street-search';
import { ZipCodeSearch } from './search/zip-code/zip-code-search';

type Tab = 'zipCode' | 'street';

const THEME_LABEL = { system: 'Sistema', light: 'Claro', dark: 'Escuro' } as const;

@Component({
  selector: 'app-root',
  imports: [ZipCodeSearch, StreetSearch, SearchResults],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly theme = inject(Theme);
  protected readonly tab = signal<Tab>('zipCode');
  protected readonly docsUrl = `${environment.apiBaseUrl}/docs`;

  private readonly zipCodeSearch = viewChild.required(ZipCodeSearch);
  private readonly streetSearch = viewChild.required(StreetSearch);

  constructor() {
    inject(AchaiApi).wakeUp();
  }

  protected themeLabel(): string {
    return THEME_LABEL[this.theme.mode()];
  }

  protected searchZipCodeExample(zipCode: string): void {
    this.tab.set('zipCode');
    this.zipCodeSearch().searchFor(zipCode);
  }

  protected searchStreetExample({ state, city, street }: StreetExample): void {
    this.tab.set('street');
    this.streetSearch().searchFor(state, city, street);
  }
}
