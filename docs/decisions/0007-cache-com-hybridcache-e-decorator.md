# 7. Cache com HybridCache e decorator

Data: 30/09/2026

Status: Aceito

## Contexto

Toda requisição ia até o ViaCEP ou o IBGE, mesmo para um CEP consultado um segundo antes. Os dados mudam muito pouco: o endereço de um CEP quase nunca muda, e município novo é raro. Cada ida às APIs externas leva centenas de milissegundos e depende de um serviço gratuito que pode ficar lento ou fora do ar.

A API roda numa instância só.

## Opções consideradas

- **Output caching**: guarda a resposta HTTP inteira por URL. Pouco código, mas `01001-000` e `01001000` virariam entradas diferentes, e não há como tratar "encontrado" e "não encontrado" de forma diferente.
- **IMemoryCache**: simples e explícito, mas sem proteção contra várias requisições iguais ao mesmo tempo, que viram várias chamadas à API externa.
- **HybridCache**: API do .NET, estável desde o 9, que junta um cache em memória (L1) com um cache distribuído opcional (L2), protege contra requisições simultâneas iguais e é a recomendação da Microsoft para projetos novos.
- **Redis** (`IDistributedCache`): compartilhado entre instâncias, mas exige infraestrutura que uma instância só não justifica.

## Decisão

HybridCache, só em memória por enquanto, aplicado com o padrão decorator.

- `CachedApi` implementa o mesmo `IApi` e envolve o `BuscarEnderecosApiRest`. O `Program.cs` entrega o decorator ao service, que não sabe que existe cache. O service e o controller não mudaram.
- A chave usa os parâmetros já normalizados pelo service (CEP sem traço, UF em maiúscula), então `01001-000` e `01001000` caem na mesma entrada.
- Tempo de vida:
  - CEP: 24 horas.
  - Busca por logradouro: 1 hora.
  - Cidades por UF: 7 dias.
- Erros esperados também ficam no cache, com o mesmo tempo de vida. Um CEP que não existe não volta ao ViaCEP a cada tentativa. O risco é um CEP recém-criado aparecer como não encontrado por até 24 horas, o que é raro e aceitável aqui.
- Exceções não ficam no cache. Se o ViaCEP cair, a próxima requisição tenta de novo.
- Como o `Result<T>` não é serializável, o valor guardado é um `ResultadoEmCache<T>`, que leva o valor ou o `Error`.

## Consequências

- Uma consulta repetida cai de cerca de 580 ms para cerca de 50 ms, medido localmente contra o ViaCEP real.
- Se a API passar a rodar em várias instâncias, basta registrar um cache distribuído (Redis, por exemplo) para o HybridCache usar como L2, sem mudar o `CachedApi`.
- O cache some quando a API reinicia, o que é aceitável para esses dados.
- Os testes do `CachedApi` usam um HybridCache de verdade com o `IApi` de dentro mockado. Os de integração conferem que `01001-000` e `01001000` chamam o ViaCEP uma vez só.
