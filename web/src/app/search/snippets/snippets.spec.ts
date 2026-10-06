import { snippet } from './snippets';

describe('snippet', () => {
  const url = 'https://achai-api.onrender.com/buscar/01001000';

  it('monta a chamada em curl, fetch e C#', () => {
    expect(snippet('curl', url)).toBe(`curl "${url}"`);
    expect(snippet('fetch', url)).toContain(`fetch('${url}')`);
    expect(snippet('csharp', url)).toContain(`GetStringAsync("${url}")`);
  });
});
