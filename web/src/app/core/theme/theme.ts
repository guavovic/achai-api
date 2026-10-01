import { DOCUMENT } from '@angular/common';
import { Injectable, effect, inject, signal } from '@angular/core';

export type ThemeMode = 'system' | 'light' | 'dark';

const STORAGE_KEY = 'achai-theme';
const NEXT: Record<ThemeMode, ThemeMode> = { system: 'light', light: 'dark', dark: 'system' };

/**
 * Tema da página. "system" segue o sistema; "light" e "dark" põem data-theme no <html>,
 * que é o que os tokens do guavovic-ui usam para forçar um tema.
 */
@Injectable({ providedIn: 'root' })
export class Theme {
  private readonly root = inject(DOCUMENT).documentElement;

  readonly mode = signal<ThemeMode>(readSaved());

  constructor() {
    effect(() => {
      const mode = this.mode();
      if (mode === 'system') this.root.removeAttribute('data-theme');
      else this.root.setAttribute('data-theme', mode);

      try {
        localStorage.setItem(STORAGE_KEY, mode);
      } catch {
        // Sem localStorage (janela anônima, por exemplo), o tema só não fica salvo.
      }
    });
  }

  cycle(): void {
    this.mode.update((mode) => NEXT[mode]);
  }
}

function readSaved(): ThemeMode {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved === 'light' || saved === 'dark' ? saved : 'system';
  } catch {
    return 'system';
  }
}
