# 24. Tokens de design dentro do front

Data: 07/10/2026

Status: Aceito

## Contexto

O front carregava os tokens de design do guavovic-ui (v0.1.0) por um link do jsDelivr no `index.html`, e a paleta Açaí ([ADR 0021](0021-paleta-acai.md)) sobrescrevia as cores por cima. Das 32 variáveis `--gv-*` usadas nas telas, 20 só existiam no guavovic-ui: espaçamento, tamanhos de texto, pesos, altura de linha, fonte mono e arredondamento.

O guavovic-ui deixou de existir. Sem ele, o site continuaria abrindo, mas sem espaçamento, sem a escala de texto e sem a fonte do código.

## Opções consideradas

- **Copiar o `tokens.css` para o front**, carregado pelo build do Angular antes do `styles.css`.
- **Escrever só as 20 variáveis usadas** dentro do `styles.css`. Menos linhas, mas mistura a base com a paleta e perde os tokens que ainda não são usados.
- **Fixar o arquivo no jsDelivr** por um commit. O CDN só serve o que existe no GitHub, então quebraria do mesmo jeito.

## Decisão

- O `tokens.css` da v0.1.0 vira `web/src/tokens.css`, com o mesmo conteúdo e só o cabeçalho reescrito.
- Entra no `angular.json` antes do `styles.css`, então a paleta Açaí continua por cima, com os mesmos nomes.
- O link do jsDelivr sai do `index.html`.

## Consequências

- O visual não muda, e o site deixa de depender de outro repositório e de um CDN para os estilos.
- Uma requisição a menos para abrir a página: os tokens vêm no mesmo CSS do build.
- O prefixo `--gv-` fica como está, para não mexer em todos os componentes.
