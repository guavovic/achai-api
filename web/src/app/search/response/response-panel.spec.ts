import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Address } from '../../core/api/achai-api';
import { AddressSearch, SearchState } from '../address-search';
import { ResponsePanel } from './response-panel';

const url = 'https://achai-api.onrender.com/buscar/22010000';

const copacabana: Address = {
  cep: '22010-000',
  logradouro: 'Avenida Atlântica',
  complemento: 'até 1020 - lado par',
  unidade: '',
  bairro: 'Copacabana',
  localidade: 'Rio de Janeiro',
  uf: 'RJ',
  estado: 'Rio de Janeiro',
  regiao: 'Sudeste',
  tipoLogradouro: 'Avenida',
  nomeLogradouro: 'Atlântica',
  enderecoFormatado:
    'Avenida Atlântica, até 1020 - lado par - Copacabana, Rio de Janeiro/RJ, CEP 22010-000',
};

describe('ResponsePanel', () => {
  const state = signal<SearchState>({ status: 'idle' });

  beforeEach(() => {
    state.set({ status: 'idle' });
    TestBed.configureTestingModule({
      imports: [ResponsePanel],
      providers: [{ provide: AddressSearch, useValue: { state } }],
    });
  });

  async function render() {
    const fixture = TestBed.createComponent(ResponsePanel);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('antes da busca, diz onde a resposta vai aparecer', async () => {
    const element = await render();

    expect(element.textContent).toContain('nenhuma requisição ainda');
    expect(element.querySelector('.json')?.textContent).toContain('a resposta da API aparece aqui');
  });

  it('mostra o status, o tempo, o resumo e o JSON da resposta', async () => {
    state.set({
      status: 'success',
      url,
      response: { status: 200, ms: 48, body: copacabana },
      addresses: [copacabana],
    });
    const element = await render();

    expect(element.querySelector('.status')?.textContent).toContain('200 OK');
    expect(element.textContent).toContain('48 ms');
    expect(element.textContent).toContain(
      'Avenida Atlântica, até 1020 - lado par - Copacabana, Rio de Janeiro/RJ, CEP 22010-000',
    );
    expect(element.querySelector('.json .key')?.textContent).toBe('"cep"');
  });

  it('conta os endereços quando vem mais de um', async () => {
    const addresses = [copacabana, { ...copacabana, cep: '22011-000' }];
    state.set({
      status: 'success',
      url,
      response: { status: 200, ms: 90, body: addresses },
      addresses,
    });
    const element = await render();

    expect(element.textContent).toContain('2 endereços');
  });

  it('diz quando a busca não encontra nada', async () => {
    state.set({
      status: 'success',
      url,
      response: { status: 200, ms: 50, body: [] },
      addresses: [],
    });
    const element = await render();

    expect(element.textContent).toContain('nenhum endereço encontrado');
  });

  it('mostra o erro com o status e o corpo do ProblemDetails', async () => {
    const problem = { status: 404, detail: 'Nenhum endereço encontrado para o CEP informado.' };
    state.set({
      status: 'error',
      url,
      message: problem.detail,
      response: { status: 404, ms: 60, body: problem },
    });
    const element = await render();

    expect(element.querySelector('.status')?.textContent).toContain('404 Not Found');
    expect(element.querySelector('[role=alert]')?.textContent).toContain(problem.detail);
    expect(element.querySelector('.json')?.textContent).toContain('"detail"');
  });

  it('avisa quando a API não responde', async () => {
    state.set({
      status: 'error',
      url,
      message: 'Não foi possível falar com a API.',
      response: null,
    });
    const element = await render();

    expect(element.querySelector('.status')?.textContent).toContain('sem resposta');
  });

  it('avisa quando o servidor demora', async () => {
    state.set({ status: 'loading', url, slow: true });
    const element = await render();

    expect(element.textContent).toContain('o servidor está acordando');
  });
});
