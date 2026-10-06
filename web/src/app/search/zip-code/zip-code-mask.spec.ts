import { maskZipCode } from './zip-code-mask';

describe('maskZipCode', () => {
  it.each([
    ['', ''],
    ['0100', '0100'],
    ['01001', '01001'],
    ['010010', '01001-0'],
    ['01001000', '01001-000'],
    ['01001-000', '01001-000'],
    [' 01001.000 ', '01001-000'],
    ['0100100099', '01001-000'],
  ])('%j vira %j', (value, masked) => {
    expect(maskZipCode(value)).toBe(masked);
  });
});
