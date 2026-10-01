# 9. Vertical Slices com Minimal APIs

Data: 30/09/2026

Status: Aceito

## Contexto

O código estava em pastas por tipo técnico (`Controllers`, `Application/Services`, `Domain/DTOs`, `Domain/Models`...), com uma funcionalidade espalhada por várias delas. A pasta `Domain` guardava coisas que não são domínio, como DTOs de resposta e modelos das APIs externas. Os nomes misturavam português e inglês e não batiam com os arquivos: `EnderecoController` dentro de `AddressController.cs`, `IEnderecoService` dentro de `IAddressService.cs`, extensão `.CS` maiúscula, projeto `BuscarEnderecos.API`. A solução ficava dentro de `src/api`, longe dos testes.

## Opções consideradas

- **N camadas organizadas**: manter as pastas por tipo técnico, só arrumando os nomes.
- **Clean Architecture**: quatro projetos (Domain, Application, Infrastructure, Api) com regras de dependência entre eles. Feita para muita regra de negócio, o que esta API não tem. Aqui seria cerimônia, e quem avalia portfólio tende a ler como over-engineering.
- **Vertical Slice Architecture**: pastas por funcionalidade, cada uma com a rota e o handler. É a recomendação da comunidade para APIs pequenas com endpoints independentes.

## Decisão

Vertical Slice Architecture, num projeto só, com Minimal APIs.

- **`Features/`**: um arquivo por endpoint, com a rota e o handler (`GetAddressByZipCode`, `SearchAddressesByStreet`, `GetCitiesByState`). A normalização da entrada (tirar o traço do CEP, UF em maiúscula), que ficava no `EnderecoService`, mora no handler da própria feature.
- **`Common/`**: o que as features compartilham: `Result`, `Error`, `AddressErrors`, a tabela de UFs, o atributo `[BrazilianState]`, o `ProblemDetails` e o handler de exceções.
- **`Infrastructure/`**: os clientes do ViaCEP, do IBGE e da BrasilAPI, e os decorators de cache e fallback. O antigo `IApi`, que misturava ViaCEP e IBGE numa classe, virou três contratos: `IZipCodeProvider` (CEP, atendido também pela BrasilAPI), `IAddressProvider` (CEP e logradouro, o ViaCEP) e `ICityProvider` (o IBGE).
- **Minimal APIs no lugar do controller.** A validação passou para o `AddValidation()` nativo do .NET 10, com os mesmos atributos, como o ADR 0005 previa.
- **Typed clients** (`AddHttpClient<ViaCepClient>`) no lugar dos clientes nomeados: cada cliente recebe o `HttpClient` direto no construtor.
- **`CancellationToken`** passa da requisição até a chamada externa. Se o cliente desistir, a chamada ao ViaCEP é cancelada.
- **Nomes de código em inglês**, seguindo as convenções do .NET: projeto `AddressLookup.Api`, namespaces iguais às pastas, um tipo por arquivo, sufixo `Async` em métodos assíncronos. Mensagens, comentários, logs e documentação continuam em português.
- **Solução na raiz** (`AddressLookup.slnx`, o formato novo do .NET 10), com `src/` e `tests/` embaixo.

### O que não mudou

Foi só refatoração. Rotas (`/buscar/...`), nomes dos campos no JSON (`cep`, `logradouro`...), códigos de erro (`Endereco.CepNaoEncontrado`) e mensagens são o contrato com o front e continuam iguais. Por isso os parâmetros dos handlers levam os nomes da rota (`cep`, `uf`, `cidade`, `logradouro`) e os DTOs de resposta usam `[JsonPropertyName]` em português.

A prova são os testes de integração: as mesmas requisições e as mesmas verificações de antes passam depois da mudança. Só os nomes das classes de teste mudaram.

## Consequências

- Uma feature nova é um arquivo novo em `Features/`, sem mexer em controller nem em service.
- 71 testes. Um a menos que antes: o teste "o fallback repassa a busca de cidades" deixou de existir, porque as cidades não passam mais pelo fallback.
- Se o contrato da API for redesenhado (rotas REST, campos em inglês, versionamento), é uma decisão própria, que muda o front junto.
