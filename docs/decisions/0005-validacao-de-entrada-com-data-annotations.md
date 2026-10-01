# 5. Validação de entrada com DataAnnotations

Data: 30/09/2026

Status: Aceito

## Contexto

A validação era feita com `if` dentro de cada action do controller. As regras estavam repetidas, uma delas checava a mesma coisa duas vezes, e uma entrada fora do formato (CEP `123`, UF `XX`) só era barrada depois de chamar o ViaCEP ou o IBGE.

A entrada da API são quatro parâmetros de rota: `cep`, `uf`, `cidade` e `logradouro`. As regras são simples: formato do CEP, UF existente e tamanho mínimo.

## Opções consideradas

- **DataAnnotations nos parâmetros**, validados automaticamente pelo `[ApiController]`, que devolve `400` com `ValidationProblemDetails`.
- **FluentValidation**, com um validator por requisição. A integração automática com o ASP.NET Core (`FluentValidation.AspNetCore`) foi descontinuada na versão 12, e a recomendação dos autores é chamar a validação manualmente.
- **Minimal APIs com a validação nativa do .NET 10** (`AddValidation()`). Ela não funciona com controllers, então adotar isso seria trocar a arquitetura, que é assunto de outra decisão.
- **Value object `Cep`** que devolve `Result<Cep>`, com a regra no domínio.

## Decisão

DataAnnotations nos parâmetros das actions.

- CEP: `[RegularExpression(@"^\d{5}-?\d{3}$")]`. Aceita com ou sem traço, e o service tira o traço antes de consultar.
- UF: atributo próprio `[Uf]` (`Validation/UfAttribute.cs`), que confere contra as 27 siglas sem diferenciar maiúscula de minúscula. O service manda a sigla em maiúscula para as APIs externas.
- Cidade e logradouro: `[MinLength(3)]`.
- Os `if` de validação saíram do controller.
- Os títulos padrão dos `ProblemDetails`, que vêm em inglês, são trocados num lugar só (`CustomizeProblemDetails` no `AddProblemDetails`), o que vale tanto para as respostas do MVC quanto para as do pipeline, como rota inexistente.

Para quatro parâmetros com regras simples, é o recurso padrão do framework, sem dependência, e a resposta sai no formato definido no ADR 0003, com o erro de cada campo em `errors`. Se o projeto migrar para Minimal APIs, os mesmos atributos funcionam com o `AddValidation()`.

## Consequências

- Entrada inválida volta `400` sem chamar o ViaCEP nem o IBGE.
- UF inexistente passou de `404` (resposta vazia do IBGE) para `400` (erro de entrada).
- O front lê a primeira mensagem de `errors` quando o erro é de validação.
- FluentValidation fica para quando houver um corpo de requisição com regras de verdade, e o value object `Cep` pode voltar junto com o fallback para a BrasilAPI.
