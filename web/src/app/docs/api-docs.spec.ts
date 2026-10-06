import { OpenApiDocument, toApiDocs, typeOf } from './api-docs';
import fixture from './openapi.fixture.json';

describe('toApiDocs', () => {
  const docs = toApiDocs(fixture as unknown as OpenApiDocument);

  it('agrupa as rotas pelas tags, na ordem do documento', () => {
    expect(docs.title).toBe('achaí-API');
    expect(docs.baseUrl).toBe('https://achai-api.onrender.com');
    expect(docs.groups.map((group) => group.name)).toEqual(['Endereços', 'Cidades']);
    expect(docs.groups[0].operations.map((operation) => operation.path)).toEqual([
      '/buscar/{cep}',
      '/buscar/{uf}/{cidade}/{logradouro}',
    ]);
  });

  it('descreve parâmetros, regras e respostas', () => {
    const zipCode = docs.groups[0].operations[0];

    expect(zipCode.id).toBe('get-buscar-cep');
    expect(zipCode.method).toBe('GET');
    expect(zipCode.parameters[0]).toMatchObject({
      name: 'cep',
      location: 'rota',
      required: true,
      type: 'string',
      rules: ['formato ^\\d{5}-?\\d{3}$'],
    });
    expect(zipCode.responses.map((response) => response.status)).toEqual([
      '200',
      '400',
      '404',
      '429',
    ]);
    expect(zipCode.responses[0]).toMatchObject({
      schema: 'AddressResponse',
      schemaRef: 'AddressResponse',
      contentType: 'application/json',
    });
  });

  it('liga ao playground só as rotas que ele testa', () => {
    const [zipCode, street] = docs.groups[0].operations;

    expect(zipCode.playground).toBe('cep');
    expect(street.playground).toBe('logradouro');
    expect(street.responses[0].schema).toBe('AddressResponse[]');
    expect(docs.groups[1].operations[0].playground).toBeNull();
  });

  it('lista os campos dos modelos', () => {
    const address = docs.schemas.find((schema) => schema.name === 'AddressResponse')!;

    expect(address.id).toBe('modelo-AddressResponse');
    expect(address.fields[0]).toEqual({ name: 'cep', type: 'string | null', required: true });
  });
});

describe('typeOf', () => {
  it('descreve listas, mapas e tipos que aceitam null', () => {
    expect(typeOf({ type: 'array', items: { type: 'string' } })).toBe('string[]');
    expect(typeOf({ type: ['null', 'integer', 'string'] })).toBe('integer | string | null');
    expect(
      typeOf({
        type: 'object',
        additionalProperties: { type: 'array', items: { type: 'string' } },
      }),
    ).toBe('{ [campo]: string[] }');
  });
});
