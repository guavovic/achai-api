import { codeTokens } from './code-tokens';

describe('codeTokens', () => {
  it('separa palavras-chave, textos e chamadas', () => {
    const tokens = codeTokens("const resposta = await fetch('https://x/buscar/01001000');");

    expect(tokens.filter((token) => token.kind !== 'plain')).toEqual([
      { text: 'const', kind: 'keyword' },
      { text: 'await', kind: 'keyword' },
      { text: 'fetch', kind: 'call' },
      { text: "'https://x/buscar/01001000'", kind: 'string' },
    ]);
    expect(tokens.map((token) => token.text).join('')).toBe(
      "const resposta = await fetch('https://x/buscar/01001000');",
    );
  });
});
