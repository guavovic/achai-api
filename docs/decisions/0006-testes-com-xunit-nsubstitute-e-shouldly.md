# 6. Testes com xUnit v3, NSubstitute e Shouldly

Data: 30/09/2026

Status: Aceito

## Contexto

O projeto não tinha testes. A lógica que mais importa está em como a API trata as respostas do ViaCEP e do IBGE (`{"erro": "true"}`, página HTML com `400`, lista vazia) e em como transforma isso em status e `ProblemDetails`. Essas regras só eram conferidas rodando a API na mão.

O `BuscarEnderecosApiRest` criava os `HttpClient` com `new` no construtor. Assim, não havia como trocar o ViaCEP por uma resposta controlada, e qualquer teste dessa classe chamaria a API real.

## Opções consideradas

- **Framework**: xUnit v3, NUnit 4, MSTest 4 e TUnit. O xUnit v3 é o padrão para projeto novo em 2026, é o que o time do ASP.NET Core usa e roda os testes em paralelo, com uma instância nova da classe por teste.
- **Mock**: NSubstitute, Moq e FakeItEasy. O Moq chegou a incluir, em 2023, um componente que coletava o e-mail do desenvolvedor no build (SponsorLink), depois removido. O NSubstitute tem a sintaxe mais enxuta.
- **Assertions**: Shouldly, FluentAssertions, AwesomeAssertions e o `Assert` do xUnit. A FluentAssertions passou a exigir licença paga para uso comercial a partir da v8.
- **APIs externas nos testes**: WireMock.Net (servidor HTTP falso completo) ou um `HttpMessageHandler` falso.

## Decisão

- **xUnit v3 + NSubstitute + Shouldly**, num projeto `tests/BuscarEnderecos.API.Tests`, com duas pastas:
  - `Unit`: `Result`, o atributo `[Uf]`, o `EnderecoService` com o `IApi` mockado e o `BuscarEnderecosApiRest` com respostas falsas do ViaCEP e do IBGE.
  - `Integration`: a API inteira em memória (`WebApplicationFactory`), com requisições HTTP de verdade conferindo status, `ProblemDetails` e se as APIs externas foram chamadas ou não.
- **`IHttpClientFactory` com clientes nomeados** (`ViaCep` e `Ibge`), registrados no `Program.cs`. Era uma mudança prevista para o item de camadas, adiantada porque sem ela não dá para testar o cliente HTTP.
- **`FakeHttpMessageHandler`** no lugar da rede: devolve a resposta configurada e guarda as requisições, para conferir a URL chamada. As respostas usadas são cópias das respostas reais das APIs (`RespostasExternas`). WireMock.Net é mais do que o projeto precisa.
- **Microsoft.Testing.Platform** como executor do `dotnet test`, ativado no `global.json`. É a plataforma que o xUnit v3 usa, e o SDK do .NET 10 não roda mais esses testes pelo VSTest.
- O CI roda `dotnet test` depois do build.

## Consequências

- 43 testes, que rodam em cerca de 3 segundos e não dependem de rede.
- Quebrar de propósito a remoção do traço do CEP ou o tratamento do `{"erro": "true"}` faz 4 testes falharem, o que mostra que eles pegam regressão de verdade.
- Uma mudança no formato de resposta do ViaCEP não é detectada pelos testes, já que eles usam respostas gravadas. Isso fica para o item de health check.
