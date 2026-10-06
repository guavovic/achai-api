# 18. Front como playground da API

Data: 06/10/2026

Status: Aceito. Substitui a parte de ícones do ADR 0016.

## Contexto

O foco do Achaí é a API. O front existe para mostrar a API funcionando, mas parecia um site de busca de CEP genérico: card centralizado, ícone ao lado de cada palavra, uma caixa tracejada com sugestões em pílula e o azul padrão de template. Não mostrava nada da API: nem a URL chamada, nem o status, nem o JSON.

## Opções consideradas

- **Ferramenta de dev**: tipografia monoespaçada, a requisição e o JSON em destaque, poucas cores e sem cards, no estilo de documentação técnica.
- **Correio brasileiro**: envelope, etiqueta e carimbo, com o resultado como etiqueta de endereçamento.
- **Editorial**: fundo claro, serifa forte no título e o endereço grande, como manchete.

## Decisão

- **Ferramenta de dev.** A página vira um playground: escolha do endpoint, parâmetros, exemplo de chamada (curl, fetch e C#) e a resposta com status HTTP, tempo e o JSON com destaque de sintaxe.
- Os erros aparecem como a API devolve, com o status e o corpo do ProblemDetails.
- Fonte JetBrains Mono em tudo. Cores só para o que tem significado: verde para o método e o 2xx, vermelho para erro, âmbar para números e literais no JSON. As cores continuam vindo dos tokens do guavovic-ui.
- **Sem ícones.** Os links e o botão de tema viram texto, e o `@lucide/angular` sai das dependências. O logo continua no cabeçalho e no favicon.
- **Atualização de 06/10/2026:** o front passa a usar só o tema claro, e o botão de tema sai.

## Consequências

- O bundle inicial fica em cerca de 71 KB comprimidos, já com o destaque de JSON e sem os ícones.
- A fonte vem do Google Fonts. Sem ela, a página cai na fonte monoespaçada do sistema.
- O resultado formatado para quem não programa ficou reduzido a uma linha de resumo acima do JSON.
