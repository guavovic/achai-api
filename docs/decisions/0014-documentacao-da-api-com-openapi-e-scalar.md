# 14. Documentação da API com OpenAPI nativo e Scalar

Data: 30/09/2026

Status: Substituído em parte pelo ADR 0020 (o Scalar saiu; o OpenAPI nativo continua)

## Contexto

A API já estava publicada, mas a única documentação das rotas era o Swagger, aberto só em desenvolvimento. Quem visita a demo ou avalia o projeto não tinha como ver as rotas, os parâmetros e as respostas sem clonar o repositório. O projeto também carregava dois geradores de OpenAPI: o `Microsoft.AspNetCore.OpenApi`, nativo, e o Swashbuckle, que fazia o trabalho de fato.

## Opções consideradas

- **Wiki do GitHub**: fácil de editar, mas fica num repositório separado, sem PR, sem CI e sem proteção de branch, então desatualiza sem ninguém notar.
- **Swagger UI (Swashbuckle) aberto em produção**: já estava no projeto, mas tem visual datado, e o .NET 9 deixou de trazer o Swashbuckle nos modelos de projeto.
- **OpenAPI nativo + Scalar**: o documento é gerado pelo próprio ASP.NET Core a partir das rotas, e o Scalar mostra uma referência moderna, com busca, exemplos de código e um cliente para testar as rotas.
- **OpenAPI nativo + Redoc**: boa leitura, mas sem testar as rotas pela página.

## Decisão

- **OpenAPI nativo** (`AddOpenApi`/`MapOpenApi`), com o documento em `/openapi/v1.json`. O **Swashbuckle sai**.
- **Scalar** (`Scalar.AspNetCore`) em `/docs`, **aberto também em produção**: a API é pública e só tem consultas, então a documentação não expõe nada que as rotas já não exponham.
- Cada rota declara resumo, descrição e as respostas possíveis (`Produces`, `ProducesValidationProblem`, `ProducesProblem`), porque os handlers devolvem `IResult` e o gerador não descobre os tipos sozinho. Os parâmetros ganham `[Description]` com exemplo; o padrão do CEP e o tamanho mínimo vêm das validações que já existiam.
- Os arquivos do Scalar são servidos pela própria API, sem CDN. Vão comprimidos (cerca de 1,3 MB) e com `ETag`, então o navegador reaproveita nas próximas visitas.
- O exemplo de código padrão é JavaScript com `fetch`, que é como o front chama a API.
- Guias de funcionamento (arquitetura, erros, cache e fallback) ficam para depois, em `docs/`, versionados com o código.

## Consequências

- A demo ganha uma página que mostra e testa a API sem instalar nada.
- O documento acompanha o código: rota nova ou validação nova aparece sozinha. Os testes conferem que as três rotas e as respostas do CEP estão no documento.
- Uma dependência a menos (Swashbuckle) e uma a mais (Scalar), que só serve arquivos estáticos.
- Os `/health` não entram no documento, porque são para a hospedagem e não para quem usa a API.
- As visitas ao `/docs` contam no limite de 60 requisições por minuto, como qualquer rota.
