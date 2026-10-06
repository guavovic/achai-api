import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PlaygroundPage } from './playground-page';

describe('PlaygroundPage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PlaygroundPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });

  async function render(endpoint?: string) {
    const fixture = TestBed.createComponent(PlaygroundPage);
    if (endpoint) fixture.componentRef.setInput('endpoint', endpoint);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function selectedTab(element: HTMLElement) {
    return element.querySelector('[role=tab][aria-selected=true]')?.id;
  }

  it('abre na busca por CEP', async () => {
    expect(selectedTab(await render())).toBe('tab-cep');
  });

  it('abre na busca por logradouro quando a URL pede', async () => {
    expect(selectedTab(await render('logradouro'))).toBe('tab-endereco');
  });
});
