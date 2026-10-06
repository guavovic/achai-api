export type SnippetLanguage = 'curl' | 'fetch' | 'csharp';

export interface BatchBody {
  ceps: string[];
}

export const SNIPPET_LANGUAGES: readonly { id: SnippetLanguage; label: string }[] = [
  { id: 'curl', label: 'curl' },
  { id: 'fetch', label: 'fetch' },
  { id: 'csharp', label: 'C#' },
];

export function snippet(language: SnippetLanguage, url: string, body?: BatchBody): string {
  return body ? postSnippet(language, url, body) : getSnippet(language, url);
}

function getSnippet(language: SnippetLanguage, url: string): string {
  switch (language) {
    case 'curl':
      return `curl "${url}"`;
    case 'fetch':
      return `const resposta = await fetch('${url}');\nconst endereco = await resposta.json();`;
    case 'csharp':
      return `using var http = new HttpClient();\nvar json = await http.GetStringAsync("${url}");`;
  }
}

function postSnippet(language: SnippetLanguage, url: string, body: BatchBody): string {
  const json = JSON.stringify(body);
  switch (language) {
    case 'curl':
      return [
        `curl -X POST "${url}" \\`,
        `  -H "Content-Type: application/json" \\`,
        `  -d '${json}'`,
      ].join('\n');
    case 'fetch':
      return [
        `const resposta = await fetch('${url}', {`,
        `  method: 'POST',`,
        `  headers: { 'Content-Type': 'application/json' },`,
        `  body: JSON.stringify(${json}),`,
        `});`,
        `const enderecos = await resposta.json();`,
      ].join('\n');
    case 'csharp': {
      const ceps = body.ceps.map((cep) => `"${cep}"`).join(', ');
      return [
        `using var http = new HttpClient();`,
        `var resposta = await http.PostAsJsonAsync("${url}", new { ceps = new[] { ${ceps} } });`,
        `var json = await resposta.Content.ReadAsStringAsync();`,
      ].join('\n');
    }
  }
}
