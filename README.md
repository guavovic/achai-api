# achaí-API

Aplicação web que busca endereços brasileiros pelo CEP ou pelo nome da rua. O back-end é uma API em C#/.NET, e o front é um app em Angular.

**Demo:** [achai-api.vercel.app](https://achai-api.vercel.app) · **Documentação da API:** [achai-api.onrender.com/docs](https://achai-api.onrender.com/docs)

<p align="center">
  <img src="docs/assets/achai-busca-por-endereco.gif" alt="No playground, a busca pela Rua XV de Novembro, em Curitiba, devolve 200 OK com 12 endereços em JSON" width="100%">
</p>

## Como foi feito

O projeto começou como uma API simples que repassava as respostas do ViaCEP e foi reconstruído em etapas, cada uma com a decisão registrada num ADR.

- **API em vertical slices:** cada rota é uma fatia completa (rota, validação e handler), e as fontes externas ficam atrás de interfaces.
- **Erros previsíveis:** CEP inexistente ou parâmetro inválido viram respostas padronizadas (ProblemDetails), com mensagens em português; erro inesperado vira um 500 genérico, com o detalhe só no log.
- **Robustez nas fontes externas:** cache em memória, timeout, retry e circuit breaker em cada chamada, e a BrasilAPI assume quando o ViaCEP falha numa busca por CEP.
- **Contrato compartilhado:** o front usa tipos gerados a partir do documento OpenAPI da API, então uma mudança na API quebra o build do front, e não a tela.
- **Testes em camadas:** unitários e de integração sem rede em todo pull request, e testes de contrato semanais contra as APIs reais, que abrem uma issue quando algo muda.
- **Interface:** um playground da API, com a escolha do endpoint, o exemplo de chamada em curl, fetch e C# e a resposta com status, tempo e JSON, em tema claro e escuro e com as cores vindas dos tokens do [guavovic-ui](https://github.com/guavovic/guavovic-ui).

## Tecnologias

- **API:** .NET 10, ASP.NET Core Minimal APIs, HybridCache, Microsoft.Extensions.Http.Resilience (Polly), OpenAPI com Scalar.
- **Front:** Angular 22 (componentes standalone, signals, sem zone.js), JetBrains Mono.
- **Fontes de dados:** ViaCEP, BrasilAPI e IBGE.
- **Testes:** xUnit v3, NSubstitute e Shouldly na API; Vitest no front.
- **Entrega:** Docker, GitHub Actions e deploy contínuo.

## Documentação

- [Guias](docs): arquitetura, tratamento de erros, cache e resiliência.
- [Decisões de arquitetura](docs/decisions): o porquê de cada escolha, com as alternativas consideradas.
- [Referência da API](https://achai-api.onrender.com/docs): rotas, parâmetros e respostas, com teste no navegador.
