export interface StreetExample {
  state: string;
  city: string;
  street: string;
}

// Exemplos do estado vazio, de várias regiões do país. Todos conferidos contra a API:
// cada CEP existe e cada busca por logradouro traz resultado.
export const ZIP_CODE_EXAMPLES = [
  '01001-000', // São Paulo/SP
  '20040-020', // Rio de Janeiro/RJ
  '70040-010', // Brasília/DF
  '30130-010', // Belo Horizonte/MG
  '80010-000', // Curitiba/PR
  '90010-000', // Porto Alegre/RS
  '88010-400', // Florianópolis/SC
  '60060-000', // Fortaleza/CE
  '69005-000', // Manaus/AM
  '66010-000', // Belém/PA
  '50030-230', // Recife/PE
  '64000-020', // Teresina/PI
  '57020-000', // Maceió/AL
] as const;

export const STREET_EXAMPLES: readonly StreetExample[] = [
  { state: 'RJ', city: 'Rio de Janeiro', street: 'Atlântica' },
  { state: 'SP', city: 'São Paulo', street: 'Paulista' },
  { state: 'BA', city: 'Salvador', street: 'Sete de Setembro' },
  { state: 'MG', city: 'Belo Horizonte', street: 'Afonso Pena' },
  { state: 'PR', city: 'Curitiba', street: 'XV de Novembro' },
  { state: 'RS', city: 'Porto Alegre', street: 'Ipiranga' },
  { state: 'PE', city: 'Recife', street: 'Boa Viagem' },
  { state: 'CE', city: 'Fortaleza', street: 'Beira Mar' },
  { state: 'PA', city: 'Belém', street: 'Nazaré' },
];

export function pickRandom<T>(items: readonly T[], random: () => number = Math.random): T {
  return items[Math.floor(random() * items.length)];
}
