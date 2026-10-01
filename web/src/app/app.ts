import { ChangeDetectionStrategy, Component, inject, signal, viewChild } from '@angular/core';
import {
  LucideBookOpen,
  LucideCodeXml,
  LucideMailbox,
  LucideMoon,
  LucideSignpost,
  LucideSun,
  LucideSunMoon,
} from '@lucide/angular';
import { environment } from '../environments/environment';
import { AchaiApi } from './core/api/achai-api';
import { Logo } from './core/logo/logo';
import { Theme } from './core/theme/theme';
import { StreetExample } from './search/examples';
import { SearchResults } from './search/results/search-results';
import { StreetSearch } from './search/street/street-search';
import { ZipCodeSearch } from './search/zip-code/zip-code-search';

type Tab = 'zipCode' | 'street';

const THEME_LABEL = { system: 'Sistema', light: 'Claro', dark: 'Escuro' } as const;

@Component({
  selector: 'app-root',
  imports: [
    Logo,
    ZipCodeSearch,
    StreetSearch,
    SearchResults,
    LucideBookOpen,
    LucideCodeXml,
    LucideMailbox,
    LucideMoon,
    LucideSignpost,
    LucideSun,
    LucideSunMoon,
  ],
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
