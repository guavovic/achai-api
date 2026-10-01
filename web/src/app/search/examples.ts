import examples from './examples.json';

export interface StreetExample {
  state: string;
  city: string;
  street: string;
}

// A lista fica em examples.json porque o teste de contrato da API também a lê,
// para conferir toda semana que cada exemplo continua trazendo resultado.
export const ZIP_CODE_EXAMPLES: readonly string[] = examples.zipCodes.map(
  (example) => example.zipCode,
);

export const STREET_EXAMPLES: readonly StreetExample[] = examples.streets;

export function pickRandom<T>(items: readonly T[], random: () => number = Math.random): T {
  return items[Math.floor(random() * items.length)];
}
