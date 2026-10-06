import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { MapPreview } from './map-preview';

describe('MapPreview', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [MapPreview],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  async function render() {
    const fixture = TestBed.createComponent(MapPreview);
    fixture.componentRef.setInput('zipCode', '01001000');
    await fixture.whenStable();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  it('só busca as coordenadas e carrega o mapa depois do clique', async () => {
    const { fixture, element } = await render();
    http.expectNone(() => true);
    expect(element.querySelector('iframe')).toBeNull();

    element.querySelector<HTMLButtonElement>('.open')!.click();
    http
      .expectOne(`${environment.apiBaseUrl}/buscar/01001000/coordenadas`)
      .flush({ cep: '01001000', latitude: -23.5475, longitude: -46.63611, fonte: 'BrasilAPI' });
    await fixture.whenStable();

    expect(element.querySelector('iframe')?.getAttribute('src')).toContain(
      'marker=-23.5475,-46.63611',
    );
  });

  it('avisa quando o CEP não tem coordenadas', async () => {
    const { fixture, element } = await render();

    element.querySelector<HTMLButtonElement>('.open')!.click();
    http
      .expectOne(`${environment.apiBaseUrl}/buscar/01001000/coordenadas`)
      .flush(null, { status: 404, statusText: 'Not Found' });
    await fixture.whenStable();

    expect(element.textContent).toContain('não há coordenadas');
  });
});
