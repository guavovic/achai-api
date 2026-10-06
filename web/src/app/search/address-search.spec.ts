import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Address } from '../core/api/achai-api';
import { AddressSearch } from './address-search';

const praçaDaSé: Address = {
  cep: '01001-000',
  logradouro: 'Praça da Sé',
  complemento: 'lado ímpar',
  unidade: '',
  bairro: 'Sé',
  localidade: 'São Paulo',
  uf: 'SP',
  estado: 'São Paulo',
  regiao: 'Sudeste',
  tipoLogradouro: 'Praça',
  nomeLogradouro: 'da Sé',
  enderecoFormatado: 'Praça da Sé, lado ímpar - Sé, São Paulo/SP, CEP 01001-000',
};

describe('AddressSearch', () => {
  let search: AddressSearch;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    search = TestBed.inject(AddressSearch);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('começa sem busca', () => {
    expect(search.state()).toEqual({ status: 'idle' });
  });

  it('busca o CEP sem o traço e guarda o endereço', () => {
    search.byZipCode('01001-000');
    expect(search.state().status).toBe('loading');
    expect(search.query()).toEqual({ cep: '01001000' });

    http.expectOne(`${environment.apiBaseUrl}/buscar/01001000`).flush(praçaDaSé);

    expect(search.state()).toEqual({
      status: 'success',
      url: `${environment.apiBaseUrl}/buscar/01001000`,
      response: { status: 200, ms: expect.any(Number), body: praçaDaSé },
      addresses: [praçaDaSé],
    });
  });

  it('busca por logradouro com cada parte codificada na URL', () => {
    search.byStreet('SP', 'São Paulo', 'Paulista');
    expect(search.query()).toEqual({ uf: 'SP', cidade: 'São Paulo', logradouro: 'Paulista' });

    http
      .expectOne(`${environment.apiBaseUrl}/buscar/SP/S%C3%A3o%20Paulo/Paulista`)
      .flush([praçaDaSé]);

    expect(search.state()).toMatchObject({ status: 'success', addresses: [praçaDaSé] });
  });

  it('mostra a mensagem de erro da API', () => {
    search.byZipCode('99999998');

    http
      .expectOne(`${environment.apiBaseUrl}/buscar/99999998`)
      .flush(
        { detail: 'Nenhum endereço encontrado para o CEP informado.' },
        { status: 404, statusText: 'Not Found' },
      );

    expect(search.state()).toEqual({
      status: 'error',
      url: `${environment.apiBaseUrl}/buscar/99999998`,
      message: 'Nenhum endereço encontrado para o CEP informado.',
      response: {
        status: 404,
        ms: expect.any(Number),
        body: { detail: 'Nenhum endereço encontrado para o CEP informado.' },
      },
    });
  });

  it('fica sem resposta quando a API não responde', () => {
    search.byZipCode('01001000');

    http
      .expectOne(`${environment.apiBaseUrl}/buscar/01001000`)
      .error(new ProgressEvent('error'), { status: 0 });

    expect(search.state()).toMatchObject({ status: 'error', response: null });
  });

  it('uma busca nova cancela a anterior', () => {
    search.byZipCode('01001000');
    const first = http.expectOne(`${environment.apiBaseUrl}/buscar/01001000`);

    search.byZipCode('22010000');
    http
      .expectOne(`${environment.apiBaseUrl}/buscar/22010000`)
      .flush({ ...praçaDaSé, cep: '22010-000' });

    expect(first.cancelled).toBe(true);
    expect(search.state()).toMatchObject({
      status: 'success',
      addresses: [{ ...praçaDaSé, cep: '22010-000' }],
    });
  });
});
