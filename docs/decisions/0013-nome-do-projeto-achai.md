# 13. Nome do projeto: Achaí

Data: 30/09/2026

Status: Aceito

## Contexto

O projeto tinha três nomes ao mesmo tempo: `address-lookup-api` no GitHub, `AddressLookup.Api` no código e `consultar-enderecos-api` na Vercel, com "Consulta de Endereços" no título da página. Antes de publicar a API no Render, onde o nome do serviço vira o endereço e não muda depois, valia escolher um nome só.

## Opções consideradas

- **Manter `address-lookup`**: descritivo e neutro, mas sem identidade.
- **Pino**, de pino de mapa: curto e sem acento, mas no Brasil também é gíria para droga, o que não combina com portfólio.
- **Logra**, **Cepinho**, **Ondé**, **Lote** e outros: ou genéricos demais, ou com acento no endereço, ou com sentido fraco.
- **Achaí**, de "acha aí": diz o que o app faz, tem cara de produto brasileiro e não tem duplo sentido.

## Decisão

- O projeto passa a se chamar **Achaí**.
- **Sem acento (`achai`) em tudo que é endereço ou identificador**: repositório, projeto da Vercel e imagem Docker. **Com acento ("Achaí")** no README e no título da página.
- **No código também**: solução `Achai.slnx`, projeto `Achai.Api`, testes `Achai.Api.Tests` e namespaces `Achai.Api.*`, para não haver dois nomes no mesmo projeto.
- `achai.vercel.app` e `achai.onrender.com` já pertencem a outras pessoas, então o front fica em **`achai-app.vercel.app`** e a API em **`achai-api.onrender.com`** (serviço `achai-api` no `render.yaml`). Os links de preview da Vercel seguem o nome do projeto (`achai-<hash>-guavovic-projects.vercel.app`), e o padrão do CORS acompanha.
- Os ADRs anteriores continuam com os nomes antigos, porque registram o que foi decidido na época.

## Consequências

- Um nome só em todo lugar, já no primeiro deploy.
- O GitHub redireciona os links antigos do repositório depois da renomeação, e o CI e a Vercel continuam ligados.
- O domínio antigo da Vercel (`consultar-enderecos-api-ten.vercel.app`) deixa de ser aceito pelo CORS.
