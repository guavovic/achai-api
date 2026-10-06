import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../environments/environment';
import { DocsPage } from './docs-page';
import fixture from './openapi.fixture.json';

describe('DocsPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [DocsPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  function render() {
    const fixture = TestBed.createComponent(DocsPage);
    fixture.detectChanges();
    TestBed.tick();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('avisa enquanto carrega o documento OpenAPI', async () => {
    const { element } = render();

    expect(element.textContent).toContain('carregando a documentação da API');
    http.expectOne(`${environment.apiBaseUrl}/openapi/v1.json`);
  });

  it('mostra as rotas e leva ao playground já no endpoint', async () => {
    const { fixture: page, element } = render();

    http.expectOne(`${environment.apiBaseUrl}/openapi/v1.json`).flush(fixture);
    await page.whenStable();

    const headings = [...element.querySelectorAll('.block h3')].map((h) => h.textContent?.trim());
    expect(headings).toContain('GET /buscar/{cep}');
    expect(element.querySelector('#get-buscar-cep .try')?.getAttribute('href')).toBe(
      '/?endpoint=cep',
    );
    expect(element.querySelector('#modelo-AddressResponse')).not.toBeNull();
  });

  it('avisa quando não consegue carregar o documento', async () => {
    const { fixture: page, element } = render();

    http
      .expectOne(`${environment.apiBaseUrl}/openapi/v1.json`)
      .flush(null, { status: 503, statusText: 'Service Unavailable' });
    await page.whenStable();

    expect(element.querySelector('[role=alert]')?.textContent).toContain(
      'não foi possível carregar',
    );
  });
});
