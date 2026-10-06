# 21. Paleta Açaí

Data: 06/10/2026

Status: Aceito

## Contexto

O front usava as cores provisórias do guavovic-ui: cinzas neutros e um azul de destaque, as mesmas de qualquer projeto que use os tokens. A achaí-API precisava de uma cor própria para o logo, o favicon e o site.

## Opções consideradas

Quatro paletas, comparadas aplicadas no logo, no favicon e numa prévia do playground e da documentação, com o contraste medido:

- **Açaí**: roxo-escuro de açaí como cor principal e amarelo de banana nos destaques. Puxa do próprio nome.
- **Placa de rua**: o azul com letra branca das placas de rua.
- **Ipê-amarelo**: amarelo nos selos e no logo, com links em ocre, porque amarelo não serve como cor de texto.
- **Mata Atlântica**: verde-escuro com verde-limão, que disputa com o verde do "200 OK".

## Decisão

- **Açaí.** Cor principal `#5b1e4e` (logo, favicon, botão Enviar, selo do método, link ativo e links), fundo `#faf8f9` levemente rosado, texto `#1f1420` e um tom claro `#f3e9f0` para o endpoint selecionado e o cabeçalho dos blocos da documentação. O amarelo fica só nos números e literais do JSON, em `#8a6200` para ter contraste no branco.
- Os valores ficam no `styles.css` do front, por cima dos tokens do guavovic-ui e com os mesmos nomes, então os componentes não mudam. O guavovic-ui continua sendo a base para outros projetos.
- O método HTTP vira um selo na cor principal, no playground e na documentação.

## Consequências

- O verde do "200 OK" e o vermelho dos erros continuam como cores de estado, separadas da cor principal.
- O favicon tem a cor escrita no arquivo e precisa ser trocado à mão se a paleta mudar.
