# 20. Documentação no front, sem o Scalar

Data: 06/10/2026

Status: Aceito. Substitui a parte do Scalar do ADR 0014.

## Contexto

A documentação em `/docs` era o Scalar, gerado a partir do OpenAPI. Funcionava, mas tinha outra identidade visual, diferente do playground (ADR 0018), trazia uma dependência a mais e fazia a API servir cerca de 1,27 MB de arquivos só para essa página. O "testar no navegador" do Scalar também ficou repetido, porque o playground já faz isso.

## Opções consideradas

- **Documentação escrita à mão**: controle total do texto, mas cada mudança na API precisaria ser lembrada também na documentação, e ela desatualiza.
- **Página no front, montada a partir do OpenAPI**: mesmo visual do playground, sempre em dia com o código, sem dependência nova.
- **Manter o Scalar com outro tema**: menor esforço, mas continua com a dependência e com outra cara.

## Decisão

- **Página `docs` no front**, que lê o `/openapi/v1.json` da API e mostra os endpoints por grupo, os parâmetros com tipo e regras, as respostas com o corpo de cada status e os modelos com os campos.
- Cada endpoint que o playground cobre tem um link "testar no playground", que abre o playground já nele (`/?endpoint=cep` ou `/?endpoint=logradouro`).
- Sai o `Scalar.AspNetCore`. O documento OpenAPI nativo continua, e o `/docs` da API passa a redirecionar para a página do front (`Docs:Url` no `appsettings.json`).
- O front ganha rotas (`/` e `/docs`), com a página de documentação carregada sob demanda.

## Consequências

- A documentação continua gerada a partir do código: uma rota, um parâmetro ou um status novo aparecem sozinhos.
- O bundle inicial do front subiu de cerca de 71 KB para 95 KB comprimidos, por causa do roteador; a página de documentação vem num arquivo separado, de cerca de 4 KB.
- Exemplos de resposta não aparecem na documentação, porque o OpenAPI não tem exemplos; quem quer ver uma resposta real usa o playground.
