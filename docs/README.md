# Documentação

## Guias

Como a API funciona hoje.

- [Arquitetura](arquitetura.md): organização do código e o caminho de uma requisição.
- [Erros](erros.md): formato das respostas de erro, códigos e como criar um erro novo.
- [Cache e resiliência](cache-e-resiliencia.md): cache, timeout, retry, circuit breaker, fallback e saúde das fontes.

## Referência da API

A lista de rotas, parâmetros, respostas e modelos fica na página de documentação do front, montada a partir do documento OpenAPI da API: [achai-api.vercel.app/docs](https://achai-api.vercel.app/docs). O documento em si fica em [`/openapi/v1.json`](https://achai-api.onrender.com/openapi/v1.json), e o `/docs` da API redireciona para a página do front.

## Decisões de arquitetura

[`decisions/`](decisions) guarda os ADRs: por que cada escolha foi feita, quais eram as alternativas e o que cada uma implica. Os guias contam **como** a API funciona; os ADRs contam **por quê**.
