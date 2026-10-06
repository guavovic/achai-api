import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { IsActiveMatchOptions, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AchaiApi } from './core/api/achai-api';
import { Logo } from './core/logo/logo';
import { Theme } from './core/theme/theme';

@Component({
  selector: 'app-root',
  imports: [Logo, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly theme = inject(Theme);
  protected readonly playgroundActive: IsActiveMatchOptions = {
    paths: 'exact',
    queryParams: 'ignored',
    fragment: 'ignored',
    matrixParams: 'ignored',
  };

  constructor() {
    inject(AchaiApi).wakeUp();
  }

  protected themeAction(): string {
    return this.theme.mode() === 'dark' ? 'Mudar para o tema claro' : 'Mudar para o tema escuro';
  }
}
