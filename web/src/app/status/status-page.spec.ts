import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { StatusPage } from './status-page';

describe('StatusPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [StatusPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('mostra a disponibilidade, a latência e as verificações de cada fonte', async () => {
    const fixture = TestBed.createComponent(StatusPage);
    fixture.detectChanges();
    TestBed.tick();

    http.expectOne(`${environment.apiBaseUrl}/status`).flush({
      desde: '2026-10-06T12:00:00Z',
      intervaloMinutos: 5,
      fontes: [
        {
          fonte: 'ViaCEP',
          disponibilidade: 50,
          latenciaMediaMs: 200,
          ultimoStatus: 'fora',
          amostras: [
            { em: '2026-10-06T12:00:00Z', status: 'ok', ms: 100 },
            { em: '2026-10-06T12:05:00Z', status: 'fora', ms: 300 },
          ],
        },
      ],
    });
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('h2')?.textContent).toContain('ViaCEP');
    expect(element.textContent).toContain('50%');
    expect(element.textContent).toContain('200 ms');
    expect(element.querySelectorAll('.timeline li')).toHaveLength(2);
    expect(element.querySelector('.chip')?.classList).toContain('fail');
  });
});
