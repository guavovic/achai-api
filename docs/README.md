# Documentação

## Guias

Como a API funciona hoje.

- [Arquitetura](arquitetura.md): organização do código e o caminho de uma requisição.
- [Erros](erros.md): formato das respostas de erro, códigos e como criar um erro novo.
- [Cache e resiliência](cache-e-resiliencia.md): cache, timeout, retry, circuit breaker, fallback e saúde das fontes.

## Referência da API

A lista de rotas, parâmetros e respostas fica na documentação interativa, gerada a partir do código: [achai-api.onrender.com/docs](https://achai-api.onrender.com/docs). Rodando localmente, ela abre em `http://localhost:5010/docs`.

## Decisões de arquitetura

[`decisions/`](decisions) guarda os ADRs: por que cada escolha foi feita, quais eram as alternativas e o que cada uma implica. Os guias contam **como** a API funciona; os ADRs contam **por quê**.
