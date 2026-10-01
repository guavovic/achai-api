import { HttpErrorResponse } from '@angular/common/http';
import { problemMessage } from './problem-message';

describe('problemMessage', () => {
  it('usa a primeira mensagem de validação por campo', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        title: 'Um ou mais campos são inválidos.',
        errors: { cep: ['O CEP deve ter 8 dígitos.'] },
      },
    });

    expect(problemMessage(error)).toBe('O CEP deve ter 8 dígitos.');
  });

  it('usa o detail quando não há erros por campo', () => {
    const error = new HttpErrorResponse({
      status: 404,
      error: {
        title: 'Não encontrado',
        detail: 'Nenhum endereço encontrado para o CEP informado.',
      },
    });

    expect(problemMessage(error)).toBe('Nenhum endereço encontrado para o CEP informado.');
  });

  it('avisa quando não consegue falar com a API', () => {
    expect(problemMessage(new HttpErrorResponse({ status: 0 }))).toContain(
      'Não foi possível falar com a API',
    );
  });

  it('tem uma mensagem para erros que não são da API', () => {
    expect(problemMessage(new Error('qualquer'))).toBe('Algo deu errado. Tente de novo.');
  });
});
