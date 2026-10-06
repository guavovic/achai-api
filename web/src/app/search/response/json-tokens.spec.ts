import { jsonTokens } from './json-tokens';

describe('jsonTokens', () => {
  it('separa chaves, textos, números e literais', () => {
    const tokens = jsonTokens({ cep: '01001-000', ddd: 11, ativo: true, unidade: null });

    expect(tokens.filter((token) => token.kind !== 'plain')).toEqual([
      { text: '"cep"', kind: 'key' },
      { text: '"01001-000"', kind: 'string' },
      { text: '"ddd"', kind: 'key' },
      { text: '11', kind: 'number' },
      { text: '"ativo"', kind: 'key' },
      { text: 'true', kind: 'literal' },
      { text: '"unidade"', kind: 'key' },
      { text: 'null', kind: 'literal' },
    ]);
  });

  it('remonta o mesmo JSON formatado', () => {
    const value = [{ logradouro: 'Rua "XV" de Novembro', numero: -1.5e3 }];

    const text = jsonTokens(value)
      .map((token) => token.text)
      .join('');

    expect(text).toBe(JSON.stringify(value, null, 2));
  });
});
