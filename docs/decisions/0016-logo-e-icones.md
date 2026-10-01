# 16. Logo e ícones

Data: 01/10/2026

Status: Aceito

## Contexto

O front novo (ADR 0015) tinha só texto: sem logo, com um favicon genérico de pino de mapa e sem ícones. A identidade visual (cores e fontes) ainda não está decidida, então o logo não pode depender de uma cor específica.

## Opções consideradas

**Logo:** quatro conceitos ligados ao nome "acha aí":
- **Lupa com pino**: o mais fácil de entender, e também o mais comum.
- **Acento que é um pino**: o acento do "í" de Achaí desenhado como um pino de mapa. O mais autoral, porque só funciona com esse nome.
- **Pino com check** ("achei!"): nítido em tamanho pequeno, mas parece um "confirmado" genérico.
- **Balão de fala** ("acha aí!"): com personalidade, mas perde nitidez em 16 px.

**Ícones:**
- **Lucide**: traço uniforme, licença livre (ISC), pacote `@lucide/angular` feito para signals e sem zone.js, com só os ícones usados entrando no build.
- **Material Symbols**: muito completo, mas carregado como fonte, com estilo mais "Google".
- **Heroicons / Phosphor**: bons, mas sem pacote oficial atualizado para o Angular atual.

## Decisão

- **Logo: o acento que é um pino.** Desenhado em SVG com uma cor só (`currentColor`), num componente (`app-logo`) que herda a cor de destaque do tema. O mesmo desenho vira o favicon.
- **Ícones: Lucide**, pelo `@lucide/angular`. O pacote antigo, `lucide-angular`, não aceita o Angular 22.
- Ícones nas abas, nos botões de busca, no botão de tema (sistema, claro e escuro), nas linhas do resultado, no botão de copiar, nos erros e no carregando. O logo aparece no cabeçalho e no estado vazio.
- Os exemplos do estado vazio passam a ser sorteados de uma lista de CEPs e ruas de várias regiões, todos conferidos contra a API.

## Consequências

- Quando a identidade visual for decidida, o logo muda de cor sozinho. O favicon é um arquivo separado e precisa ter a cor trocada à mão.
- O bundle inicial cresceu cerca de 5 KB comprimido com os ícones.
- O Lucide não tem ícones de marcas, então o link do GitHub usa um ícone de código.
