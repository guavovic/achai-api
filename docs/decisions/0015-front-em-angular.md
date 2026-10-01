# 15. Front em Angular

Data: 01/10/2026

Status: Aceito

## Contexto

O front original era uma página com HTML, CSS e JavaScript com jQuery e jQuery UI, que ficava em `src/`, junto da API. Ela funcionava, mas tinha problemas: duas colunas que não se adaptavam bem ao celular, uma caixa de resultado vazia antes da primeira busca, sem favicon e sem testes. Os tipos das respostas da API também não existiam no front, então uma mudança na API só aparecia como erro na tela.

## Opções consideradas

- **Angular**: framework completo, com injeção de dependência, formulários reativos e `HttpClient`. Comum em vagas que pedem .NET ou Java no back-end. Para uma tela só, traz mais estrutura e um bundle maior.
- **React com Vite e TypeScript**: o mais pedido no mercado e leve para uma tela, mas o portfólio já tem um projeto em React.
- **TypeScript sem framework, com Vite**: mínimo, mas mostra pouco de framework de front.
- **Manter o JavaScript puro, só tirando o jQuery**: o menor esforço, sem ganho de estrutura nem de testes.

## Decisão

- **Angular 22**, num projeto próprio em `web/`, separado da API.
- **Recursos atuais do Angular**: componentes standalone, signals para o estado, `httpResource` para a lista de cidades, controle de fluxo novo (`@if`, `@for`, `@switch`), sem zone.js e com detecção de mudanças `OnPush`.
- **Estado da busca num serviço com signals** (`AddressSearch`), compartilhado pelos dois formulários e pela lista de resultados. Uma busca nova cancela a anterior, para uma resposta atrasada não sobrescrever a mais recente.
- **Tipos gerados do OpenAPI da API** (`npm run api:types`, com o `openapi-typescript`). O gerador roda pelo `npx` com versão fixa, fora das dependências do projeto, porque ainda não declara suporte ao TypeScript 6.
- **Tela em uma coluna, com abas** "Por CEP" e "Por endereço". Os dois formulários ficam montados, então trocar de aba não apaga o que foi digitado. Antes da primeira busca, a área de resultado mostra exemplos clicáveis.
- **Visual pelos tokens do guavovic-ui**, com tema do sistema, claro ou escuro, escolhido por um botão e salvo no navegador.
- **Testes com Vitest**, o padrão do Angular 22, e um job novo no CI que compila e testa o front.
- O front antigo (`src/index.html`, `src/js`, `src/styles`) sai.

## Consequências

- O portfólio ganha um par completo .NET + Angular, com tipos compartilhados pelo contrato OpenAPI.
- O bundle inicial é de cerca de 70 KB comprimido, maior que o da página antiga, mas pequeno para um app Angular.
- O front passa a ter etapa de build: a Vercel compila o projeto da pasta `web/`.
- Para atualizar os tipos depois de mudar a API, é preciso rodar a API localmente e o `npm run api:types`.
