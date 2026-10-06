export interface CitySuggestion {
  name: string;
  before: string;
  match: string;
  after: string;
}

const LIMIT = 50;

export function suggestCities(names: readonly string[], query: string): CitySuggestion[] {
  const wanted = fold(query.trim());
  if (!wanted) return names.slice(0, LIMIT).map((name) => split(name, 0, 0));

  const starts: CitySuggestion[] = [];
  const contains: CitySuggestion[] = [];
  for (const name of names) {
    const index = fold(name).indexOf(wanted);
    if (index === 0) starts.push(split(name, 0, wanted.length));
    else if (index > 0) contains.push(split(name, index, wanted.length));
  }

  return [...starts, ...contains].slice(0, LIMIT);
}

function fold(text: string): string {
  return text.normalize('NFD').replace(/\p{M}/gu, '').toLocaleLowerCase('pt-BR');
}

function split(name: string, start: number, length: number): CitySuggestion {
  return {
    name,
    before: name.slice(0, start),
    match: name.slice(start, start + length),
    after: name.slice(start + length),
  };
}
