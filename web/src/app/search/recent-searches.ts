import { Injectable, signal } from '@angular/core';
import { SearchQuery } from './address-search';

export const RECENT_SEARCHES_KEY = 'achai-recentes';
const LIMIT = 5;

@Injectable({ providedIn: 'root' })
export class RecentSearches {
  private readonly items = signal<SearchQuery[]>(load());

  readonly list = this.items.asReadonly();

  add(query: SearchQuery): void {
    const key = keyOf(query);
    this.items.update((items) =>
      [query, ...items.filter((item) => keyOf(item) !== key)].slice(0, LIMIT),
    );
    save(this.items());
  }

  clear(): void {
    this.items.set([]);
    save([]);
  }
}

export function isZipCodeQuery(query: SearchQuery): query is { cep: string } {
  return 'cep' in query;
}

export function isConsensusQuery(query: SearchQuery): query is { consenso: string } {
  return 'consenso' in query;
}

export function isBatchQuery(query: SearchQuery): query is { ceps: string } {
  return 'ceps' in query;
}

function keyOf(query: SearchQuery): string {
  if (isZipCodeQuery(query)) return query.cep;
  if (isBatchQuery(query)) return `lote|${query.ceps}`;
  if (isConsensusQuery(query)) return `consenso|${query.consenso}`;
  return [query.uf, query.cidade, query.logradouro].join('|').toLocaleLowerCase('pt-BR');
}

function load(): SearchQuery[] {
  try {
    const saved: unknown = JSON.parse(localStorage.getItem(RECENT_SEARCHES_KEY) ?? '[]');
    return Array.isArray(saved) ? (saved as SearchQuery[]).slice(0, LIMIT) : [];
  } catch {
    return [];
  }
}

function save(items: SearchQuery[]): void {
  try {
    localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(items));
  } catch {}
}
