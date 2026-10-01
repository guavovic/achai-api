# 8. Resiliência e fallback para a BrasilAPI

Data: 30/09/2026

Status: Aceito

## Contexto

A API depende do ViaCEP e do IBGE, dois serviços gratuitos. Se um deles ficasse lento, a requisição esperava sem limite. Se caísse, a API devolvia 500. O README citava a BrasilAPI, mas nenhum código a usava.

Testando a BrasilAPI (`/api/cep/v2/{cep}`):

- Ela responde `404` para CEP inexistente e `400` para formato inválido, o que dá para traduzir para os mesmos erros do ViaCEP.
- As fontes discordam: o CEP `99999999`, que o ViaCEP diz não existir, a BrasilAPI devolve como um endereço em Sarandi, PR (base open-cep).
- Ela não tem busca por logradouro, e a lista de cidades dela é pior que a do IBGE (nomes em maiúscula, com sufixos como "(ACRE)").
- A resposta de CEP não traz complemento, unidade, nome do estado nem região.

## Opções consideradas

- **`Microsoft.Extensions.Http.Resilience`** (`AddStandardResilienceHandler`): pacote oficial em cima do Polly v8, com timeout, retry e circuit breaker por `HttpClient`.
- **Polly direto com a estratégia de Fallback**: o fallback ficaria no nível HTTP e teria que fabricar uma resposta no formato do ViaCEP a partir da BrasilAPI.
- **Hedging** (`AddStandardHedgingHandler`): só serve quando as duas fontes falam a mesma API.
- **`try/catch` sem biblioteca**: sem retry nem circuit breaker.

## Decisão

- **`AddStandardResilienceHandler` nos três clientes** (ViaCEP, IBGE e BrasilAPI), com os valores ajustados para uma busca que precisa ser rápida:
  - 2 segundos por tentativa e 6 segundos no total (o padrão do pacote é 30 segundos).
  - Até 2 novas tentativas, começando em 200 ms.
  - Circuit breaker abre com 50% de falhas a partir de 5 requisições e fica aberto por 30 segundos. O padrão exige 100 requisições em 30 segundos, o que nunca aconteceria no volume desta API.
- **Fallback como decorator** (`FallbackApi`), no mesmo padrão do cache. A cadeia montada no `Program.cs` fica: cache → fallback → ViaCEP/IBGE.
- O fallback vale **só para a busca por CEP** e **só quando o ViaCEP falha**: `HttpRequestException`, timeout, circuito aberto ou JSON inválido. Um "não encontrado" do ViaCEP é resposta válida e não aciona a BrasilAPI, porque as fontes discordam sobre CEPs inexistentes. Erros de programação também não acionam, para não esconder bug.
- **Busca por logradouro e cidades ficam só com a resiliência**, sem fallback.
- O endereço da BrasilAPI é convertido para o mesmo formato do ViaCEP. O nome do estado e a região vêm da tabela `UnidadesFederativas`, que também passou a ser a fonte do atributo `[Uf]`. Complemento e unidade ficam vazios.
- O README agora descreve o que o código faz.

## Consequências

- Com o ViaCEP fora do ar, a busca por CEP continua respondendo. Medido localmente: cerca de 1,1 s pela BrasilAPI, e o resultado vai para o cache.
- Uma falha passageira do ViaCEP é resolvida pelo retry, sem chegar à BrasilAPI.
- Um endereço que veio da BrasilAPI fica no cache por 24 horas como qualquer outro, mesmo sem complemento.
- No pior caso (as duas fontes fora), a requisição leva até cerca de 12 segundos antes do 500.
- Os testes de integração rodam com o pipeline de resiliência de verdade e conferem as 3 tentativas no ViaCEP antes do fallback.
