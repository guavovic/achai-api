export interface OpenApiSchema {
  $ref?: string;
  type?: string | string[];
  items?: OpenApiSchema;
  properties?: Record<string, OpenApiSchema>;
  additionalProperties?: OpenApiSchema | boolean;
  required?: string[];
  pattern?: string;
  minLength?: number;
  maxLength?: number;
}

interface OpenApiParameter {
  name: string;
  in: string;
  required?: boolean;
  description?: string;
  schema?: OpenApiSchema;
}

interface OpenApiOperation {
  tags?: string[];
  summary?: string;
  description?: string;
  parameters?: OpenApiParameter[];
  responses?: Record<
    string,
    { description?: string; content?: Record<string, { schema?: OpenApiSchema }> }
  >;
}

export interface OpenApiDocument {
  info: { title: string; description?: string };
  servers?: { url: string }[];
  tags?: { name: string }[];
  paths: Record<string, Record<string, OpenApiOperation>>;
  components?: { schemas?: Record<string, OpenApiSchema> };
}

export type PlaygroundEndpoint = 'cep' | 'logradouro' | 'lote' | 'consenso';

export interface DocsParameter {
  name: string;
  location: string;
  required: boolean;
  type: string;
  rules: string[];
  description: string;
}

export interface DocsResponse {
  status: string;
  description: string;
  contentType: string;
  schema: string;
  schemaRef: string | null;
}

export interface DocsOperation {
  id: string;
  method: string;
  path: string;
  summary: string;
  description: string;
  parameters: DocsParameter[];
  responses: DocsResponse[];
  playground: PlaygroundEndpoint | null;
}

export interface DocsGroup {
  name: string;
  operations: DocsOperation[];
}

export interface DocsSchema {
  id: string;
  name: string;
  fields: { name: string; type: string; required: boolean }[];
}

export interface ApiDocs {
  title: string;
  description: string;
  baseUrl: string;
  groups: DocsGroup[];
  schemas: DocsSchema[];
}

const PLAYGROUND_ENDPOINTS: Record<string, PlaygroundEndpoint> = {
  '/buscar/{cep}': 'cep',
  '/buscar/{uf}/{cidade}/{logradouro}': 'logradouro',
  '/buscar/lote': 'lote',
  '/buscar/{cep}/consenso': 'consenso',
};

const LOCATIONS: Record<string, string> = {
  path: 'rota',
  query: 'query',
  header: 'cabeçalho',
};

export function toApiDocs(document: OpenApiDocument): ApiDocs {
  const groups = new Map<string, DocsOperation[]>(
    (document.tags ?? []).map((tag) => [tag.name, []]),
  );

  for (const [path, methods] of Object.entries(document.paths)) {
    for (const [method, operation] of Object.entries(methods)) {
      const tag = operation.tags?.[0] ?? 'Outros';
      if (!groups.has(tag)) groups.set(tag, []);
      groups.get(tag)!.push(toOperation(path, method, operation));
    }
  }

  return {
    title: document.info.title,
    description: document.info.description ?? '',
    baseUrl: document.servers?.[0]?.url.replace(/\/$/, '') ?? '',
    groups: [...groups].map(([name, operations]) => ({ name, operations })),
    schemas: Object.entries(document.components?.schemas ?? {}).map(([name, schema]) => ({
      id: schemaId(name),
      name,
      fields: Object.entries(schema.properties ?? {}).map(([field, fieldSchema]) => ({
        name: field,
        type: typeOf(fieldSchema),
        required: schema.required?.includes(field) ?? false,
      })),
    })),
  };
}

export function schemaId(name: string): string {
  return `modelo-${name}`;
}

function toOperation(path: string, method: string, operation: OpenApiOperation): DocsOperation {
  return {
    id: `${method}-${path}`
      .replace(/[^a-z0-9]+/gi, '-')
      .replace(/-+$/, '')
      .toLowerCase(),
    method: method.toUpperCase(),
    path,
    summary: operation.summary ?? '',
    description: operation.description ?? '',
    parameters: (operation.parameters ?? []).map((parameter) => ({
      name: parameter.name,
      location: LOCATIONS[parameter.in] ?? parameter.in,
      required: parameter.required ?? false,
      type: parameter.schema ? typeOf(parameter.schema) : 'string',
      rules: parameter.schema ? rulesOf(parameter.schema) : [],
      description: parameter.description ?? '',
    })),
    responses: Object.entries(operation.responses ?? {}).map(([status, response]) => {
      const [contentType, media] = Object.entries(response.content ?? {})[0] ?? ['', {}];
      return {
        status,
        description: response.description ?? '',
        contentType,
        schema: media.schema ? typeOf(media.schema) : '',
        schemaRef: media.schema ? refName(media.schema.items ?? media.schema) : null,
      };
    }),
    playground: PLAYGROUND_ENDPOINTS[path] ?? null,
  };
}

function refName(schema: OpenApiSchema): string | null {
  return schema.$ref?.split('/').pop() ?? null;
}

export function typeOf(schema: OpenApiSchema): string {
  const ref = refName(schema);
  if (ref) return ref;

  const types = [schema.type ?? 'object'].flat();
  const nullable = types.includes('null');
  const named = types
    .filter((type) => type !== 'null')
    .map((type) => {
      if (type === 'array' && schema.items) return `${typeOf(schema.items)}[]`;
      if (type === 'object' && typeof schema.additionalProperties === 'object')
        return `{ [campo]: ${typeOf(schema.additionalProperties)} }`;
      return type;
    });

  return [...named, ...(nullable ? ['null'] : [])].join(' | ');
}

function rulesOf(schema: OpenApiSchema): string[] {
  return [
    schema.pattern ? `formato ${schema.pattern}` : '',
    schema.minLength ? `mín. ${schema.minLength} caracteres` : '',
    schema.maxLength ? `máx. ${schema.maxLength} caracteres` : '',
  ].filter(Boolean);
}
