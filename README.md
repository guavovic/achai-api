# Achaí

Aplicação web com back-end em C#/.NET que busca endereços brasileiros pelo CEP ou pelo logradouro, com uma interface simples para consultar os resultados.

## Tecnologias

C# / .NET 10, JavaScript, jQuery e CSS.

- **ViaCEP** para as buscas por CEP e por logradouro, com a **BrasilAPI** como alternativa para CEP quando o ViaCEP está fora do ar ou lento.
- **IBGE** para a lista de cidades de cada estado.
- Timeout, retry e circuit breaker em toda chamada externa (`Microsoft.Extensions.Http.Resilience`), e cache em memória (`HybridCache`).

As decisões de arquitetura ficam registradas em [`docs/decisions`](docs/decisions).

## Como rodar

Precisa do SDK do .NET 10.

1. Suba a API:

   ```bash
   cd src/api
   dotnet run
   ```

   Ela escuta em `http://localhost:5010`, e o Swagger abre em `http://localhost:5010/swagger`.

2. Abra o `src/index.html` no navegador.

### Com Docker

```bash
docker build -t achai .
docker run --rm -p 5010:8080 -e ASPNETCORE_ENVIRONMENT=Development achai
```

A imagem usa o runtime chiseled do .NET 10: sem shell, sem gerenciador de pacotes e rodando sem root. Sem `ASPNETCORE_ENVIRONMENT=Development`, o container roda em modo produção, e o CORS só libera o front publicado. Se a variável de ambiente `PORT` estiver definida (como fazem hospedagens como o Render), a API escuta nela em vez da 8080.

## Saúde e limites

- `GET /health`: liveness. Diz só se o processo da API está de pé, sem chamar nada externo.
- `GET /health/ready`: confere ViaCEP, BrasilAPI e IBGE. Uma fonte fora do ar deixa o status `Degraded`, porque a API continua respondendo.
- Cada IP pode fazer 60 requisições por minuto. Acima disso, a API responde `429` com o cabeçalho `Retry-After`. O `/health` não entra no limite.
- Em produção, só as origens de `Cors:AllowedOrigins` e os links de preview da Vercel que casam com `Cors:AllowedOriginPatterns` (`appsettings.json`) podem chamar a API pelo navegador. Em desenvolvimento, qualquer origem pode.

## Publicação

- **API:** [Render](https://render.com), plano grátis, descrita no [`render.yaml`](render.yaml). Todo merge na `main` que mexe na API só é publicado depois que o CI passa, e o Render espera o `/health` responder antes de trocar o tráfego.
- **Front:** Vercel, com um link de preview para cada pull request.
- O front escolhe a API pelo próprio endereço: aberto do disco ou de `localhost`, chama `http://localhost:5010`; publicado, chama o endereço do Render (`src/js/config.js`).
- No plano grátis, a API dorme depois de 15 minutos sem acesso e leva até um minuto para acordar. O front mostra um aviso quando a resposta demora mais de 3 segundos.

## Como testar

Na raiz do repositório:

```bash
dotnet test
```

Os testes unitários e de integração usam xUnit v3, NSubstitute e Shouldly. ViaCEP, IBGE e BrasilAPI são trocados por handlers HTTP falsos, então os testes não precisam de rede.

## Estrutura do projeto

A API usa Vertical Slice Architecture com Minimal APIs:

```
src/api/
  Features/         um arquivo por endpoint (rota + handler)
    Addresses/      GetAddressByZipCode, SearchAddressesByStreet
    Cities/         GetCitiesByState
  Common/           Result, erros, validação, ProblemDetails
  Infrastructure/   clientes do ViaCEP, IBGE e BrasilAPI, decorators de cache e fallback
tests/Achai.Api.Tests/
  Unit/             handlers, clientes e decorators
  Integration/      a API inteira em memória (WebApplicationFactory)
```

## Como usar

**Busca por CEP:** digite um CEP válido (por exemplo, 88350250) e clique em Buscar. O endereço aparece no painel da direita.

**Busca por estado e cidade:** escolha o estado e a cidade, digite o logradouro (no mínimo 3 caracteres) e clique em Buscar. Os resultados aparecem em lista.
