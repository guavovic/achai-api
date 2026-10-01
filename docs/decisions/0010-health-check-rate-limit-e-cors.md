# 10. Health check, rate limit e CORS

Data: 30/09/2026

Status: Aceito

## Contexto

A API vai ser publicada (front na Vercel, API no Render ou no Azure). Faltavam três coisas que uma API pública precisa:

- Um endereço para a hospedagem saber se a API está de pé.
- Proteção contra quem faz requisições demais, que consumiria o plano gratuito e as cotas do ViaCEP e da BrasilAPI.
- Um CORS coerente. O de antes era contraditório: `WithOrigins("http://localhost:3000")` seguido de `AllowAnyOrigin()`, que anulava o primeiro.

## Opções consideradas

- **Health check**: um endpoint só, ou liveness e readiness separados. A recomendação da comunidade é a liveness não depender de nada externo. Se ela checasse o ViaCEP, uma queda dele faria a hospedagem reiniciar a API à toa, e a API nem precisa dele para responder, por causa do fallback.
- **Rate limit**: o middleware nativo do ASP.NET Core (janela fixa, janela deslizante, token bucket ou concorrência) ou uma biblioteca como AspNetCoreRateLimit.
- **CORS**: uma lista de origens permitidas, ou qualquer origem. Qualquer origem é defensável numa API pública e sem login (o próprio ViaCEP é assim), mas costuma ser lida como descuido. Vale lembrar que CORS não protege a API de abuso: só impede que um site de terceiros a chame pelo navegador de quem o visita. Contra abuso, a proteção é o rate limit.

## Decisão

- **`/health` (liveness)**: não roda nenhuma checagem, só responde se o processo está de pé. É o endereço que a hospedagem vai consultar. Fica fora do rate limit.
- **`/health/ready` (readiness)**: testa o ViaCEP e a BrasilAPI buscando o CEP da Praça da Sé, e o IBGE buscando as cidades do Acre, com 3 segundos de limite cada. Uma fonte fora deixa o status `Degraded`, não `Unhealthy`, porque a API continua respondendo. A resposta em JSON traz só nome, status e duração de cada fonte; a mensagem de erro fica no log.
- **Rate limit nativo**, sem biblioteca: 60 requisições por minuto por IP, em janela fixa. Quem passa do limite recebe `429` (o padrão do middleware é `503`, que estaria errado) com `ProblemDetails` em português e o cabeçalho `Retry-After`, que o middleware não manda sozinho.
- **`ForwardedHeaders`**: atrás do proxy da hospedagem, o IP da conexão é o do proxy, e o limite "por IP" viraria um limite único para todo mundo. O `X-Forwarded-For` passa a valer. Nenhum proxy é fixado, porque o IP dele muda; o `ForwardLimit` padrão usa só a entrada que o próprio proxy acrescenta, que o cliente não consegue forjar.
- **CORS por lista**: em produção, só as origens de `Cors:AllowedOrigins` no `appsettings.json` (hoje, o front na Vercel). Em desenvolvimento, qualquer origem, para o `index.html` aberto direto do disco continuar funcionando. Só o método `GET` é liberado, porque a API só tem consultas.
- O CORS roda antes do rate limit, para o front conseguir ler a resposta `429`.

## Consequências

- A hospedagem não reinicia a API quando uma fonte externa cai.
- Medido localmente em produção: `/health/ready` respondeu as três fontes saudáveis em menos de 600 ms; a 61ª requisição do mesmo IP no mesmo minuto voltou `429` com `Retry-After: 60`.
- Os links de preview da Vercel, que mudam a cada PR, ainda não estão na lista de origens. Isso entra no item de deploy.
- Se a API rodar em várias instâncias, cada uma terá o próprio contador. Para um limite global seria preciso guardar o contador fora do processo (Redis, por exemplo).
