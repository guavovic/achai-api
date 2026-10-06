export type SnippetLanguage = 'curl' | 'fetch' | 'csharp';

export const SNIPPET_LANGUAGES: readonly { id: SnippetLanguage; label: string }[] = [
  { id: 'curl', label: 'curl' },
  { id: 'fetch', label: 'fetch' },
  { id: 'csharp', label: 'C#' },
];

export function snippet(language: SnippetLanguage, url: string): string {
  switch (language) {
    case 'curl':
      return `curl "${url}"`;
    case 'fetch':
      return `const resposta = await fetch('${url}');\nconst endereco = await resposta.json();`;
    case 'csharp':
      return `using var http = new HttpClient();\nvar json = await http.GetStringAsync("${url}");`;
  }
}
