export type CodeTokenKind = 'string' | 'keyword' | 'call' | 'plain';

export interface CodeToken {
  text: string;
  kind: CodeTokenKind;
}

const TOKEN = /("[^"]*"|'[^']*')|\b(curl|const|await|using|var|new)\b|\b([A-Za-z_]\w*)(?=\()/g;

export function codeTokens(code: string): CodeToken[] {
  const tokens: CodeToken[] = [];
  let last = 0;

  for (const match of code.matchAll(TOKEN)) {
    if (match.index > last) tokens.push({ text: code.slice(last, match.index), kind: 'plain' });
    const kind: CodeTokenKind = match[1] ? 'string' : match[2] ? 'keyword' : 'call';
    tokens.push({ text: match[0], kind });
    last = match.index + match[0].length;
  }

  if (last < code.length) tokens.push({ text: code.slice(last), kind: 'plain' });
  return tokens;
}
