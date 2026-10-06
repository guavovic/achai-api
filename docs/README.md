# Documentação

## Guias

Como a API funciona hoje.

- [Arquitetura](arquitetura.md): organização do código e o caminho de uma requisição.
- [Erros](erros.md): formato das respostas de erro, códigos e como criar um erro novo.
- [Cache e resiliência](cache-e-resiliencia.md): cache, timeout, retry, circuit breaker, fallback e saúde das fontes.

## Referência da API

A lista de rotas, parâmetros, respostas e modelos fica na página de documentação do front, montada a partir do documento OpenAPI da API: [achai-api.vercel.app/docs](https://achai-api.vercel.app/docs). O documento em si fica em [`/openapi/v1.json`](https://achai-api.onrender.com/openapi/v1.json), e o `/docs` da API redireciona para a página do front.

## Servidor MCP

A API também é um servidor [MCP](https://modelcontextprotocol.io), para assistentes de IA (Claude, Cursor e outros) consultarem endereços. O endereço é `https://achai-api.onrender.com/mcp`, com transporte HTTP, sem chave. As ferramentas são `buscar_cep`, `buscar_logradouro`, `listar_cidades` e `distancia_entre_ceps`, com as mesmas regras, o mesmo cache e o mesmo limite de requisições da API.

No Claude Code:

```
claude mcp add --transport http achai-api https://achai-api.onrender.com/mcp
```

Em clientes configurados por JSON:

```json
{
  "mcpServers": {
    "achai-api": { "type": "http", "url": "https://achai-api.onrender.com/mcp" }
  }
}
```

## Decisões de arquitetura

[`decisions/`](decisions) guarda os ADRs: por que cada escolha foi feita, quais eram as alternativas e o que cada uma implica. Os guias contam **como** a API funciona; os ADRs contam **por quê**.
