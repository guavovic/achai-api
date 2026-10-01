import { HttpErrorResponse } from '@angular/common/http';
import type { components } from './schema';

type ValidationProblem = components['schemas']['HttpValidationProblemDetails'];

export function problemMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Algo deu errado. Tente de novo.';

  if (error.status === 0)
    return 'Não foi possível falar com a API. Confira sua conexão e tente de novo.';

  const problem = error.error as ValidationProblem | null;
  const fieldError = Object.values(problem?.errors ?? {}).flat()[0];

  return fieldError ?? problem?.detail ?? problem?.title ?? `Erro ${error.status}. Tente de novo.`;
}
