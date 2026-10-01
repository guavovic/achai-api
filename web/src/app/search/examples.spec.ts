import { STREET_EXAMPLES, ZIP_CODE_EXAMPLES, pickRandom } from './examples';

describe('exemplos', () => {
  it('sorteia pelo número aleatório, do primeiro ao último item', () => {
    const items = ['a', 'b', 'c'];

    expect(pickRandom(items, () => 0)).toBe('a');
    expect(pickRandom(items, () => 0.5)).toBe('b');
    expect(pickRandom(items, () => 0.999)).toBe('c');
  });

  it('não repete CEP e todos estão no formato aceito', () => {
    expect(new Set(ZIP_CODE_EXAMPLES).size).toBe(ZIP_CODE_EXAMPLES.length);
    for (const zipCode of ZIP_CODE_EXAMPLES) expect(zipCode).toMatch(/^\d{5}-\d{3}$/);
  });

  it('cada busca por logradouro tem estado, cidade e rua válidos', () => {
    for (const { state, city, street } of STREET_EXAMPLES) {
      expect(state).toMatch(/^[A-Z]{2}$/);
      expect(city.length).toBeGreaterThanOrEqual(3);
      expect(street.length).toBeGreaterThanOrEqual(3);
    }
  });
});
