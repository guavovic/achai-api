# achaí-API

Projeto de estudo de 2026 sobre arquitetura de APIs. É uma aplicação web que busca endereços brasileiros pelo CEP ou pelo nome da rua: o back-end é uma API em C#/.NET e o front é um app em Angular, que funciona como playground da API.

Começou como um repasse simples do ViaCEP e foi reconstruído em etapas, cada uma com a decisão registrada num ADR: rotas em vertical slices, erros padronizados com ProblemDetails, cache, timeout, retry e circuit breaker nas fontes externas, consenso entre o ViaCEP e a BrasilAPI, tipos do front gerados do OpenAPI, SDKs para .NET e TypeScript, um servidor MCP para assistentes de IA e testes de contrato semanais contra as APIs reais.

<img src="docs/assets/achai-busca-por-endereco.gif" alt="No playground, a busca pela Rua XV de Novembro, em Curitiba, devolve 200 OK com 12 endereços em JSON" width="100%">

A demo continua no ar em [achai-api.vercel.app](https://achai-api.vercel.app).

**Tecnologias:** .NET 10, ASP.NET Core, Angular, Docker e GitHub Actions.

As decisões do projeto estão em [`docs/decisions`](docs/decisions), e os guias de arquitetura, cache e erros em [`docs`](docs).

---

<sub>Licença [MIT](LICENSE) · Feito por [Gustavo Victor](https://github.com/guavovic)</sub>
