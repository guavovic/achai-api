# Cache e resiliência

A API depende de três fontes externas que podem ficar lentas ou fora do ar. Três mecanismos trabalham juntos para isso afetar o mínimo possível quem está usando:

1. **Cache**: a maior parte das consultas nem chega às fontes.
2. **Resiliência HTTP**: cada chamada externa tem tempo máximo, nova tentativa e circuit breaker.
3. **Fallback**: se o ViaCEP falhar numa busca por CEP, a BrasilAPI responde no lugar.

## Cache

Usa o `HybridCache` do .NET, só em memória ([ADR 0007](decisions/0007-cache-com-hybridcache-e-decorator.md)).

| Consulta | Chave | Validade |
|---|---|---|
| CEP | `cep:{cep}` | 24 horas |
| Logradouro | `logradouro:{uf}:{cidade}:{logradouro}` | 1 hora |
| Cidades de um estado | `cidades:{uf}` | 7 dias |

- **"Não encontrado" também fica no cache.** Um CEP que não existe continua não existindo, e guardar isso evita bater no ViaCEP toda vez.
- **Exceção não fica no cache.** Se a fonte falhou, a próxima requisição tenta de novo.
- Como o `Result<T>` não é serializável, o cache guarda um `CachedResult<T>`, com o valor ou o erro, e converte de volta na leitura.
- O `HybridCache` evita o *cache stampede*: se várias requisições pedem o mesmo CEP ao mesmo tempo e ele não está no cache, só uma vai à fonte, e as outras esperam o resultado dela.
- O cache fica em memória, então some quando a API reinicia. A primeira busca depois disso vai às fontes de novo.

## Resiliência HTTP

Os três clientes usam o `AddStandardResilienceHandler` (Polly, via `Microsoft.Extensions.Http.Resilience`), com valores ajustados para uma busca que precisa ser rápida ([ADR 0008](decisions/0008-resiliencia-e-fallback-para-a-brasilapi.md)):

| Mecanismo | Configuração | Padrão do pacote |
|---|---|---|
| Timeout por tentativa | 2 s | 10 s |
| Timeout total | 6 s | 30 s |
| Retry | 2 novas tentativas, com espera exponencial a partir de 200 ms | 3 novas tentativas, a partir de 2 s |
| Circuit breaker | abre com 50% de falhas em pelo menos 5 requisições, por 30 s | abre com 10% de falhas em pelo menos 100 requisições, por 5 s |

O mínimo do circuit breaker caiu de 100 para 5 porque, no volume desta API, 100 requisições em 30 segundos nunca acontecem, e o circuito nunca abriria. Com o circuito aberto, as chamadas falham na hora, sem esperar timeout, e o fallback entra direto.

## Fallback para a BrasilAPI

O `FallbackAddressProvider` chama o ViaCEP e, **só se ele falhar**, a BrasilAPI.

- **Conta como falha:** erro de rede, timeout, circuito aberto ou resposta que não é JSON.
- **Não conta como falha:** "CEP não encontrado". As fontes discordam sobre CEPs que não existem (o `99999999` não existe no ViaCEP, mas a BrasilAPI devolve um endereço), então o "não encontrado" do ViaCEP é a resposta final.
- **Só vale para CEP.** A BrasilAPI não tem busca por logradouro, e a lista de cidades dela é pior que a do IBGE.
- A BrasilAPI não devolve o nome do estado nem a região. A API preenche os dois a partir da UF, com a tabela `BrazilianStates`.

Cada uso do fallback gera um aviso no log, com o CEP, para dar para ver com que frequência o ViaCEP está falhando.

## Saúde das fontes

O `GET /health/ready` testa as três fontes e diz como cada uma está:

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "viacep", "status": "Healthy", "durationMs": 9 },
    { "name": "brasilapi", "status": "Healthy", "durationMs": 120 },
    { "name": "ibge", "status": "Healthy", "durationMs": 85 }
  ]
}
```

Uma fonte fora do ar deixa o status `Degraded`, e não `Unhealthy`, porque a API continua respondendo, com fallback ou com erro tratado. Já o `GET /health` não testa nada externo: só diz se o processo está de pé, e é ele que a hospedagem usa para saber se a API está no ar.
