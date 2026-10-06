import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { environment } from '../../environments/environment';
import { PlaygroundPage } from './playground-page';

describe('PlaygroundPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PlaygroundPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  async function render(inputs: Record<string, string> = {}) {
    const fixture = TestBed.createComponent(PlaygroundPage);
    for (const [name, value] of Object.entries(inputs)) fixture.componentRef.setInput(name, value);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function selectedTab(element: HTMLElement) {
    return element.querySelector('[role=tab][aria-selected=true]')?.id;
  }

  it('abre na busca por CEP, sem buscar nada', async () => {
    const element = await render();

    expect(selectedTab(element)).toBe('tab-cep');
    http.expectNone(() => true);
  });

  it('abre na busca por logradouro quando a URL pede', async () => {
    expect(selectedTab(await render({ endpoint: 'logradouro' }))).toBe('tab-endereco');
  });

  it('busca o CEP que veio no link', async () => {
    await render({ cep: '01001000' });

    http.expectOne(`${environment.apiBaseUrl}/buscar/01001000`);
  });

  it('busca o logradouro que veio no link', async () => {
    const fixture = TestBed.createComponent(PlaygroundPage);
    fixture.componentRef.setInput('uf', 'SP');
    fixture.componentRef.setInput('cidade', 'São Paulo');
    fixture.componentRef.setInput('logradouro', 'Paulista');
    TestBed.tick();
    http.expectOne(`${environment.apiBaseUrl}/buscar/cidades/SP`).flush([]);
    await fixture.whenStable();

    expect(selectedTab(fixture.nativeElement)).toBe('tab-endereco');
    http.expectOne(`${environment.apiBaseUrl}/buscar/SP/S%C3%A3o%20Paulo/Paulista`);
  });

  it('põe a busca no endereço da página', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');

    await render({ cep: '01001000' });

    expect(navigate).toHaveBeenCalledWith(
      [],
      expect.objectContaining({ queryParams: { cep: '01001000' }, replaceUrl: true }),
    );
  });
});
