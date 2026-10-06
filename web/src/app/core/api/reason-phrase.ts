const REASON_PHRASES: Record<number, string> = {
  200: 'OK',
  400: 'Bad Request',
  404: 'Not Found',
  429: 'Too Many Requests',
  500: 'Internal Server Error',
  502: 'Bad Gateway',
  503: 'Service Unavailable',
  504: 'Gateway Timeout',
};

export function reasonPhrase(status: number): string {
  return REASON_PHRASES[status] ?? '';
}
