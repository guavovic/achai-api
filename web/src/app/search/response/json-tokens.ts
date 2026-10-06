export type JsonTokenKind = 'key' | 'string' | 'number' | 'literal' | 'plain';

export interface JsonToken {
  text: string;
  kind: JsonTokenKind;
}

const TOKEN =
  /("(?:\\.|[^"\\])*")(\s*:)?|\b(?:true|false|null)\b|-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?/g;

export function jsonTokens(value: unknown): JsonToken[] {
  const json = JSON.stringify(value, null, 2) ?? '';
  const tokens: JsonToken[] = [];
  let last = 0;

  for (const match of json.matchAll(TOKEN)) {
    const [text, quoted, colon] = match;
    if (match.index > last) tokens.push({ text: json.slice(last, match.index), kind: 'plain' });

    if (quoted) {
      tokens.push({ text: quoted, kind: colon ? 'key' : 'string' });
      if (colon) tokens.push({ text: colon, kind: 'plain' });
    } else {
      tokens.push({ text, kind: /^[-\d]/.test(text) ? 'number' : 'literal' });
    }
    last = match.index + text.length;
  }

  if (last < json.length) tokens.push({ text: json.slice(last), kind: 'plain' });
  return tokens;
}
