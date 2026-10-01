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

  it.each(['01001000', '01001-000'])('busca o CEP %s', async (zipCode) => {
    const { fixture, element } = render();

    type(element, zipCode);
    element.querySelector('button')!.click();
    await fixture.whenStable();

    expect(byZipCode).toHaveBeenCalledWith(zipCode);
    expect(element.querySelector('.field-error')).toBeNull();
  });
});
