import { TestBed } from '@angular/core/testing';
import { AddressSearch } from '../address-search';
import { ZipCodeSearch } from './zip-code-search';

describe('ZipCodeSearch', () => {
  const byZipCode = vi.fn();

  beforeEach(() => {
    byZipCode.mockClear();
    TestBed.configureTestingModule({
      imports: [ZipCodeSearch],
      providers: [{ provide: AddressSearch, useValue: { byZipCode } }],
    });
  });

  function render() {
    const fixture = TestBed.createComponent(ZipCodeSearch);
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  function type(element: HTMLElement, value: string) {
    const input = element.querySelector<HTMLInputElement>('#cep')!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  it('não busca e mostra o erro quando o CEP está fora do formato', async () => {
    const { fixture, element } = render();

    type(element, '123');
    element.querySelector('button')!.click();
    await fixture.whenStable();

    expect(byZipCode).not.toHaveBeenCalled();
    expect(element.querySelector('.field-error')?.textContent).toContain('8 dígitos');
  });

  it.each(['01001000', '01001-000', '01001 000'])(
    'põe o traço e busca sozinho ao completar o CEP %s',
    async (zipCode) => {
      const { fixture, element } = render();

      type(element, zipCode);
      await fixture.whenStable();

      expect(element.querySelector<HTMLInputElement>('#cep')!.value).toBe('01001-000');
      expect(byZipCode).toHaveBeenCalledExactlyOnceWith('01001-000');
      expect(element.querySelector('.field-error')).toBeNull();
    },
  );

  it('não busca de novo enquanto o CEP não muda', async () => {
    const { fixture, element } = render();

    type(element, '01001000');
    type(element, '01001-000');
    await fixture.whenStable();

    expect(byZipCode).toHaveBeenCalledTimes(1);
  });

  it('o botão busca de novo o mesmo CEP', async () => {
    const { fixture, element } = render();

    type(element, '01001000');
    element.querySelector('button')!.click();
    await fixture.whenStable();

    expect(byZipCode).toHaveBeenCalledTimes(2);
  });

  it('busca o exemplo clicado', () => {
    const { fixture } = render();

    fixture.componentInstance.searchFor('22010000');

    expect(byZipCode).toHaveBeenCalledExactlyOnceWith('22010-000');
  });
});
