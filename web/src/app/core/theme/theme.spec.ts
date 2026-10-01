import { TestBed } from '@angular/core/testing';
import { THEME_STORAGE_KEY, Theme } from './theme';

describe('Theme', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  function create(): Theme {
    const theme = TestBed.inject(Theme);
    TestBed.tick();
    return theme;
  }

  it('começa no escuro', () => {
    create();

    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBeNull();
  });

  it('guarda a escolha do claro', () => {
    const theme = create();

    theme.toggle();
    TestBed.tick();

    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('light');
  });

  it('volta ao escuro e esquece a escolha', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'light');
    const theme = create();
    expect(theme.mode()).toBe('light');

    theme.toggle();
    TestBed.tick();

    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBeNull();
  });
});
