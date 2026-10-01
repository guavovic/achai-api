# Erros

Toda resposta de erro da API segue o mesmo formato, o **ProblemDetails** ([RFC 9457](https://www.rfc-editor.org/rfc/rfc9457)), com títulos em português. Quem consome a API trata qualquer erro do mesmo jeito.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Não encontrado",
  "status": 404,
  "detail": "Nenhum endereço encontrado para o CEP informado.",
  "code": "Endereco.CepNaoEncontrado"
}
```

- `detail` é a mensagem para mostrar ao usuário. O front usa esse campo.
- `code` é estável e serve para o código tomar decisões, sem depender do texto.

## Esperado ou inesperado

A API separa os erros em dois tipos, e cada um segue um caminho diferente ([ADR 0003](decisions/0003-tratamento-de-erros-com-result-e-problem-details.md)).

**Esperado** é o que faz parte do funcionamento normal: CEP que não existe, UF sem cidades. Esses erros **não são exceções**. O cliente devolve um `Result<T>` com o erro, e o handler o converte em resposta:

```csharp
return result.Match(
    address => TypedResults.Ok(AddressResponse.From(address)),
    error => error.ToProblem());
```

**Inesperado** é o que não deveria acontecer: uma fonte externa fora do ar sem fallback, um bug. Esses viram exceção e são capturados pelo `GlobalExceptionHandler`, que:

- registra o erro completo no log, com a rota;
- responde um 500 genérico, sem mostrar detalhes internos para quem chamou.

## Tabela de respostas

| Status | Quando | `code` |
|---|---|---|
| 400 | Parâmetro fora do formato (CEP sem 8 dígitos, UF que não existe, cidade ou logradouro com menos de 3 caracteres). A resposta traz `errors`, com as mensagens por campo | — |
| 400 | O ViaCEP recusou o CEP ou a busca, mesmo depois da validação | `Endereco.CepInvalido`, `Endereco.BuscaInvalida` |
| 404 | O CEP tem o formato certo, mas não existe | `Endereco.CepNaoEncontrado` |
| 404 | O IBGE não tem cidades para a UF | `Cidade.UfNaoEncontrada` |
| 404 | A rota não existe | — |
| 429 | O IP passou do limite de requisições. Vem com o cabeçalho `Retry-After`, que diz em quantos segundos tentar de novo | — |
| 500 | Erro inesperado | — |

Uma busca por logradouro **sem resultados** não é erro: responde `200` com uma lista vazia.

## Onde fica cada parte

| Arquivo | Papel |
|---|---|
| `Common/Results/Result.cs` | O `Result<T>`: sucesso com valor ou falha com `Error` |
| `Common/Results/Error.cs` | O `Error` (código, descrição e tipo: `Validation` ou `NotFound`) |
| `Common/AddressErrors.cs` | Os erros conhecidos da API, com os códigos do contrato |
| `Common/Http/ProblemExtensions.cs` | Converte `Error` em ProblemDetails e traduz os títulos padrão |
| `Common/Http/GlobalExceptionHandler.cs` | Captura exceções e responde 500 |

## Criando um erro novo

1. Declare o erro em `AddressErrors`, com um código no formato `Area.Motivo`, em português.
2. Devolva o erro no cliente ou no handler (`return AddressErrors.MeuErro;`). A conversão implícita transforma o `Error` em `Result<T>`.
3. Se for um tipo novo (nem validação nem "não encontrado"), acrescente o tipo em `ErrorType` e o status correspondente em `ProblemExtensions.ToProblem`.
