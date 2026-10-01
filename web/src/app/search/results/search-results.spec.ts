import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AddressSearch, SearchState } from '../address-search';
import { ZIP_CODE_EXAMPLES } from '../examples';
import { SearchResults } from './search-results';

describe('SearchResults', () => {
  const state = signal<SearchState>({ status: 'idle' });

  beforeEach(() => {
    state.set({ status: 'idle' });
    TestBed.configureTestingModule({
      imports: [SearchResults],
      providers: [{ provide: AddressSearch, useValue: { state } }],
    });
  });

  async function render() {
    const fixture = TestBed.createComponent(SearchResults);
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('antes da busca, mostra os exemplos e avisa qual foi clicado', async () => {
    const { fixture, element } = await render();
    const clicked = vi.fn();
    fixture.componentInstance.zipCodeExample.subscribe(clicked);

    element.querySelector<HTMLButtonElement>('.example')!.click();

    expect(element.textContent).toContain('Digite um CEP');
    expect(clicked).toHaveBeenCalledWith(fixture.componentInstance.examples.zipCode);
    expect(ZIP_CODE_EXAMPLES).toContain(fixture.componentInstance.examples.zipCode);
  });

  it('mostra cada endereço encontrado', async () => {
    state.set({
      status: 'success',
      addresses: [
        {
          cep: '22010-000',
          logradouro: 'Avenida Atlântica',
          complemento: 'até 1020 - lado par',
          unidade: '',
          bairro: 'Copacabana',
          localidade: 'Rio de Janeiro',
          uf: 'RJ',
          estado: 'Rio de Janeiro',
          regiao: 'Sudeste',
        },
      ],
    });
    const { element } = await render();

    expect(element.querySelector('h2')?.textContent).toContain('Avenida Atlântica');
    expect(element.textContent).toContain('Rio de Janeiro/RJ · Sudeste');
    expect(element.textContent).toContain('CEP 22010-000');
  });

  it('diz quando a busca não encontra nada', async () => {
    state.set({ status: 'success', addresses: [] });
    const { element } = await render();

    expect(element.textContent).toContain('Nenhum endereço encontrado');
  });

  it('mostra o erro como alerta', async () => {
    state.set({ status: 'error', message: 'Nenhum endereço encontrado para o CEP informado.' });
    const { element } = await render();

    expect(element.querySelector('[role=alert]')?.textContent).toContain(
      'Nenhum endereço encontrado',
    );
  });

  it('avisa quando o servidor demora', async () => {
    state.set({ status: 'loading', slow: true });
    const { element } = await render();

    expect(element.textContent).toContain('O servidor está acordando');
  });
});
