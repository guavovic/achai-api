import { TestBed } from '@angular/core/testing';
import { AddressSearch } from '../address-search';
import { BatchSearch, parseZipCodes } from './batch-search';

describe('parseZipCodes', () => {
  it('aceita vírgula, ponto e vírgula, espaço e quebra de linha', () => {
    expect(parseZipCodes('01001-000, 22010000;\n30130905  ')).toEqual([
      '01001-000',
      '22010000',
      '30130905',
    ]);
  });
});

describe('BatchSearch', () => {
  const byZipCodes = vi.fn();

  beforeEach(() => {
    byZipCodes.mockClear();
    TestBed.configureTestingModule({
      imports: [BatchSearch],
      providers: [{ provide: AddressSearch, useValue: { byZipCodes } }],
    });
  });

  function render() {
    const fixture = TestBed.createComponent(BatchSearch);
    fixture.detectChanges();
    return fixture;
  }

  it('busca a lista colada', () => {
    const fixture = render();

    fixture.componentInstance.searchFor(['01001000', '22010000']);

    expect(byZipCodes).toHaveBeenCalledWith(['01001000', '22010000']);
  });

  it('não busca nada vazio nem mais de 20 CEPs', async () => {
    const fixture = render();

    fixture.componentInstance.submit();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('.field-error')?.textContent).toContain(
      'pelo menos um CEP',
    );

    fixture.componentInstance.searchFor(Array.from({ length: 21 }, () => '01001000'));
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('.field-error')?.textContent).toContain(
      'O limite é 20',
    );
    expect(byZipCodes).not.toHaveBeenCalled();
  });
});
