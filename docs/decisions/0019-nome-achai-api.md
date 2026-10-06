# 19. Nome Achaí API

Data: 06/10/2026

Status: Aceito. Complementa o ADR 0013.

## Contexto

O foco do projeto é a API, e o front virou um playground dela (ADR 0018). O nome "Achaí" sozinho soava como um site de busca de CEP.

## Decisão

- O produto passa a se chamar **Achaí API**: no README, no título da página, no cabeçalho do front e no título da documentação (OpenAPI e Scalar).
- O repositório vira `guavovic/achai-api`. O endereço antigo continua redirecionando.
- Não mudam: os nomes no código (`Achai.Api`, `Achai.slnx`) e o serviço do Render (`achai-api.onrender.com`). O domínio do front passou de `achai-app.vercel.app` para `achai-api.vercel.app`, e o CORS acompanhou.

## Consequências

- Os links antigos do GitHub seguem funcionando pelo redirecionamento, mas o remoto das cópias locais deve ser atualizado.
