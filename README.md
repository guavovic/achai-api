<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/assets/logo-escuro.svg">
    <img src="docs/assets/logo-claro.svg" alt="Logo do Achaí: a letra í com o acento em forma de pino de mapa" width="96" height="96">
  </picture>
</p>

<h1 align="center">Achaí</h1>

Aplicação web que busca endereços brasileiros pelo CEP ou pelo nome da rua. O back-end é uma API em C#/.NET, e o front é um app em Angular.

**Demo:** [achai-app.vercel.app](https://achai-app.vercel.app) · **Documentação da API:** [achai-api.onrender.com/docs](https://achai-api.onrender.com/docs)

## Como foi feito

O projeto começou como uma API simples que repassava as respostas do ViaCEP e foi reconstruído em etapas, cada uma com a decisão registrada num ADR.

- **API em vertical slices:** cada rota é uma fatia completa (rota, validação e handler), e as fontes externas ficam atrás de interfaces.
- **Erros previsíveis:** CEP inexistente ou parâmetro inválido viram respostas padronizadas (ProblemDetails), com mensagens em português; erro inesperado vira um 500 genérico, com o detalhe só no log.
- **Robustez nas fontes externas:** cache em memória, timeout, retry e circuit breaker em cada chamada, e a BrasilAPI assume quando o ViaCEP falha numa busca por CEP.
- **Contrato compartilhado:** o front usa tipos gerados a partir do documento OpenAPI da API, então uma mudança na API quebra o build do front, e não a tela.
- **Testes em camadas:** unitários e de integração sem rede em todo pull request, e testes de contrato semanais contra as APIs reais, que abrem uma issue quando algo muda.
- **Interface:** uma tela com abas, estados de vazio, carregando e erro, tema claro e escuro, e visual vindo dos tokens do [guavovic-ui](https://github.com/guavovic/guavovic-ui).

## Tecnologias

- **API:** .NET 10, ASP.NET Core Minimal APIs, HybridCache, Microsoft.Extensions.Http.Resilience (Polly), OpenAPI com Scalar.
- **Front:** Angular 22 (componentes standalone, signals, sem zone.js), Lucide para os ícones.
- **Fontes de dados:** ViaCEP, BrasilAPI e IBGE.
- **Testes:** xUnit v3, NSubstitute e Shouldly na API; Vitest no front.
- **Entrega:** Docker, GitHub Actions e deploy contínuo.

## Documentação

- [Guias](docs): arquitetura, tratamento de erros, cache e resiliência.
- [Decisões de arquitetura](docs/decisions): o porquê de cada escolha, com as alternativas consideradas.
- [Referência da API](https://achai-api.onrender.com/docs): rotas, parâmetros e respostas, com teste no navegador.
