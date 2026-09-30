# 3. Tratamento de erros com Result e ProblemDetails

Data: 30/09/2026

Status: Aceito

## Contexto

Os erros andavam dentro de um `ResponseDTO<T>`, com o status code da API externa e um `ExpandoObject` para o corpo do erro. Na prática:

- CEP que não existe voltava `200` com endereço vazio. O ViaCEP responde `200 {"erro": "true"}` nesse caso, e a API repassava como sucesso.
- CEP inválido (`/buscar/123`) quebrava a requisição. O ViaCEP responde `400` com uma página HTML, a desserialização do JSON estourava, um `catch` vazio engolia a exceção, o status code ficava `0` e o `StatusCode(0)` falhava.
- Dois dos três métodos tinham `catch` vazio, então falhas de verdade sumiam.
- Cada endpoint devolvia erro num formato diferente: texto solto, `{ message }` ou o corpo da API externa.

## Opções consideradas

- **Exceção para tudo**, convertida em status code por um handler global. Simples, mas "CEP não encontrado" é um resultado esperado, não uma exceção, e lançar exceção para isso esconde o fluxo do método.
- **Result para tudo**, inclusive falha de rede. Toda camada teria que tratar casos em que não pode fazer nada.
- **Híbrido**: `Result<T>` para os erros esperados do domínio e um `IExceptionHandler` global para os inesperados.

Para o tipo Result em si: uma biblioteca (ErrorOr, FluentResults, Ardalis.Result) ou um tipo pequeno no próprio projeto.

## Decisão

Modelo híbrido, com o formato de resposta da RFC 9457 (`ProblemDetails`).

- `Result<T>` e `Error` ficam em `Domain/Results`. Um `Error` tem código, descrição e tipo (`Validation` ou `NotFound`). Os erros conhecidos ficam em `Domain/Errors/EnderecoErrors.cs`.
- O cliente do ViaCEP e do IBGE traduz as manias deles para esses erros: `400` → entrada inválida, `{"erro": "true"}` → CEP não encontrado, lista de cidades vazia no IBGE → UF não encontrada. Busca de endereço sem resultado continua `200 []`, porque busca vazia não é erro.
- O controller converte um `Error` em `ProblemDetails` (`ToProblem`): `Validation` → 400, `NotFound` → 404, com o código do erro na extensão `code`.
- Todo o resto (falha de rede, `5xx` da API externa, JSON inválido) é exceção. O `GlobalExceptionHandler` registra no log e devolve um 500 genérico, sem detalhe interno.
- O `UseStatusCodePages` faz os erros do próprio framework, como rota inexistente, saírem no mesmo formato.

O tipo Result foi escrito no projeto. São umas 40 linhas, e trazer uma dependência para isso iria contra o raciocínio do ADR 0002.

## Consequências

- Toda resposta de erro tem o mesmo formato e o content type `application/problem+json`. O front lê a mensagem do `detail`.
- A assinatura dos métodos mostra quais chamadas podem falhar de forma esperada.
- Falha da API externa vira um 500 genérico por enquanto. Devolver `502`/`503` e tentar de novo fica para o item de resiliência (fallback para a BrasilAPI), que vai rever isso.
- A validação de entrada continua sendo os `if` do controller, agora devolvendo `ProblemDetails`. Uma abordagem de validação de verdade é a próxima decisão.
