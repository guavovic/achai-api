import { TestBed } from '@angular/core/testing';
import { RECENT_SEARCHES_KEY, RecentSearches } from './recent-searches';

describe('RecentSearches', () => {
  beforeEach(() => localStorage.clear());

  function create() {
    TestBed.resetTestingModule();
    return TestBed.inject(RecentSearches);
  }

  it('guarda a busca mais nova primeiro, sem repetir', () => {
    const recent = create();

    recent.add({ cep: '01001000' });
    recent.add({ uf: 'SP', cidade: 'São Paulo', logradouro: 'Paulista' });
    recent.add({ cep: '01001000' });
    recent.add({ uf: 'SP', cidade: 'são paulo', logradouro: 'PAULISTA' });

    expect(recent.list()).toEqual([
      { uf: 'SP', cidade: 'são paulo', logradouro: 'PAULISTA' },
      { cep: '01001000' },
    ]);
  });

  it('fica só com as 5 últimas', () => {
    const recent = create();

    for (const cep of ['1', '2', '3', '4', '5', '6']) recent.add({ cep });

    expect(recent.list().map((query) => ('cep' in query ? query.cep : ''))).toEqual([
      '6',
      '5',
      '4',
      '3',
      '2',
    ]);
  });

  it('lembra das buscas ao abrir a página de novo e limpa quando pedido', () => {
    create().add({ cep: '01001000' });

    const reopened = create();
    expect(reopened.list()).toEqual([{ cep: '01001000' }]);

    reopened.clear();
    expect(create().list()).toEqual([]);
  });

  it('ignora o que estiver estragado no navegador', () => {
    localStorage.setItem(RECENT_SEARCHES_KEY, '{nao e json');

    expect(create().list()).toEqual([]);
  });
});
