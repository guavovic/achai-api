import { DOCUMENT } from '@angular/common';
import { Injectable, effect, inject, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

export const THEME_STORAGE_KEY = 'achai-theme';

@Injectable({ providedIn: 'root' })
export class Theme {
  private readonly root = inject(DOCUMENT).documentElement;

  readonly mode = signal<ThemeMode>(readSaved());

  constructor() {
    effect(() => {
      const mode = this.mode();
      this.root.setAttribute('data-theme', mode);

      try {
        if (mode === 'light') localStorage.setItem(THEME_STORAGE_KEY, mode);
        else localStorage.removeItem(THEME_STORAGE_KEY);
      } catch {
        // Sem localStorage, a escolha vale só até fechar a página.
      }
    });
  }

  toggle(): void {
    this.mode.update((mode) => (mode === 'dark' ? 'light' : 'dark'));
  }
}

function readSaved(): ThemeMode {
  try {
    return localStorage.getItem(THEME_STORAGE_KEY) === 'light' ? 'light' : 'dark';
  } catch {
    return 'dark';
  }
}
