# Achaí

Aplicação web que busca endereços brasileiros pelo CEP ou pelo logradouro. O back-end é uma API em C#/.NET, e o front é uma interface simples para consultar os resultados.

**Demo:** [achai-app.vercel.app](https://achai-app.vercel.app) · **Documentação da API:** [achai-api.onrender.com/docs](https://achai-api.onrender.com/docs)

## Tecnologias

- **API:** .NET 10, ASP.NET Core Minimal APIs, organizada em vertical slices.
- **Fontes de dados:** ViaCEP para CEP e logradouro, BrasilAPI como reserva para CEP, e IBGE para a lista de cidades.
- **Robustez:** cache em memória, timeout, retry e circuit breaker nas chamadas externas, erros padronizados (ProblemDetails) e limite de requisições por IP.
- **Front:** HTML, CSS e JavaScript, com os tokens visuais do [guavovic-ui](https://github.com/guavovic/guavovic-ui).
- **Qualidade:** testes unitários e de integração, CI no GitHub Actions e imagem Docker.

## Como rodar

Precisa do SDK do .NET 10.

1. Suba a API:

   ```bash
   cd src/api
   dotnet run
   ```

   Ela escuta em `http://localhost:5010`, e a documentação interativa abre em `http://localhost:5010/docs`.

2. Abra o `src/index.html` no navegador.

### Com Docker

```bash
docker build -t achai .
docker run --rm -p 5010:8080 -e ASPNETCORE_ENVIRONMENT=Development achai
```

## Como testar

Na raiz do repositório:

```bash
dotnet test
```

As fontes externas são simuladas nos testes, então eles rodam sem rede.

## Estrutura

```
src/api/
  Features/         um arquivo por endpoint (rota + handler)
  Common/           tipos compartilhados, erros e validação
  Infrastructure/   clientes das fontes externas, cache e fallback
src/                front (HTML, CSS e JavaScript)
tests/              testes unitários e de integração
docs/               guias e decisões de arquitetura
```

## Documentação

- [Guias](docs): arquitetura, tratamento de erros, cache e resiliência.
- [Decisões de arquitetura](docs/decisions): o porquê de cada escolha, com as alternativas consideradas.
- [Referência da API](https://achai-api.onrender.com/docs): rotas, parâmetros e respostas, com teste no navegador.
