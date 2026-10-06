import createClient from 'openapi-fetch';
import type { paths } from './schema.js';

export type { components, paths } from './schema.js';

export const DEFAULT_BASE_URL = 'https://achai-api.onrender.com';

export function createAchaiClient(baseUrl: string = DEFAULT_BASE_URL) {
  return createClient<paths>({ baseUrl });
}
