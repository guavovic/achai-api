# 23. SDKs gerados do OpenAPI

Data: 06/10/2026

Status: Aceito

## Contexto

Quem usa a achaí-API escreve à mão as chamadas HTTP e os tipos das respostas. Um cliente pronto, em .NET e em TypeScript, reduz isso a uma linha, mas só vale se não ficar desatualizado em relação à API.

## Opções consideradas

- **Clientes escritos à mão**: controle total, mas desatualizam a cada rota ou campo novo.
- **NSwag (.NET) e openapi-generator**: maduros, mas com suporte parcial ao OpenAPI 3.1, que a API usa (tipos como `["null", "string"]`).
- **Kiota (.NET) e openapi-typescript com openapi-fetch (TypeScript)**: entendem o 3.1; o Kiota é da Microsoft, e o openapi-fetch é um cliente de poucos KB que usa direto os tipos gerados.

## Decisão

- O documento OpenAPI passa a ser gerado no build da API (`Microsoft.Extensions.ApiDescription.Server`) em `sdk/openapi.json`, sem precisar subir o servidor. O front também gera os tipos a partir dele.
- **.NET**: pacote `Achai.Client`, gerado pelo Kiota, com `AchaiClient.Create()` para começar.
- **TypeScript**: pacote `achai-api`, com os tipos do openapi-typescript e `createAchaiClient()` em cima do openapi-fetch.
- O CI gera tudo de novo a cada pull request e falha se algo mudou sem ser commitado. Um teste usa o cliente .NET gerado contra a API em memória.
- A cada release, um workflow publica os dois pacotes com a versão da tag, por **Trusted Publishing** (OIDC do GitHub Actions), sem chave de API guardada. No npm, a primeira publicação usa o secret `NPM_TOKEN`, porque o Trusted Publishing só pode ser ligado num pacote que já existe; depois disso o token pode ser apagado.

## Consequências

- Mudou a API, é preciso gerar os SDKs de novo antes do merge; o CI avisa.
- No NuGet, a política de Trusted Publishing precisa existir na conta antes da primeira release.
