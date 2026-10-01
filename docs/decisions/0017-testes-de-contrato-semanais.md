# 17. Testes de contrato semanais com as APIs externas

Data: 01/10/2026

Status: Aceito

## Contexto

A API depende do ViaCEP, da BrasilAPI e do IBGE. Os testes existentes trocam essas fontes por respostas falsas (ADR 0006), então continuam passando mesmo se uma delas mudar o formato da resposta. Os exemplos que o front sorteia no estado vazio (ADR 0016) também dependem do ViaCEP: um CEP que deixar de existir vira um exemplo que volta vazio.

## Opções consideradas

- **Testes de integração contra as APIs reais**: chamam as fontes com os próprios clientes da API e conferem se a resposta ainda vira um endereço completo.
- **Pact (contrato guiado pelo consumidor)**: o consumidor publica o contrato, e o fornecedor o verifica do lado dele. Serve quando os dois lados são do mesmo time; o ViaCEP não vai rodar os contratos de quem o consome.
- **Snapshot da resposta inteira** (biblioteca Verify): acusa qualquer diferença, inclusive dados que mudam sem o formato mudar, como o nome de um bairro.

Para a frequência, diária ou semanal.

## Decisão

- **Testes de integração contra as APIs reais**, num projeto separado (`tests/Achai.Api.ContractTests`), com os clientes montados como na API (`AddInfrastructure`), inclusive o pipeline de resiliência.
- Os testes são **explícitos** (`[Fact(Explicit = true)]`): o `dotnet test` normal e o CI das PRs os mostram como ignorados, e eles só rodam com `-- --explicit only`. Assim, uma instabilidade de terceiros não trava uma PR.
- **O que é conferido**: CEP conhecido e inexistente no ViaCEP e na BrasilAPI, busca por logradouro no ViaCEP, lista de cidades no IBGE e **cada exemplo do front**. A lista de exemplos passou para `web/src/app/search/examples.json`, lida pelo front e pelos testes.
- **Workflow semanal** (segunda, 9:00 em Brasília), com disparo manual. Essas APIs mudam raramente, e uma quebra no meio da semana aparece antes nos logs e no uso da demo.
- **Quando falha, o workflow abre uma issue** com o rótulo `contrato`, ou comenta na que já estiver aberta, com o link da execução.

## Consequências

- Uma mudança de formato numa fonte externa, ou um exemplo do front que deixou de funcionar, vira uma issue em até uma semana.
- São cerca de 30 chamadas por semana às APIs públicas.
- Uma instabilidade momentânea também abre a issue. A própria issue orienta a rodar o workflow de novo antes de investigar.
- O GitHub desliga workflows agendados de repositórios sem commits por 60 dias; se o projeto ficar parado, o workflow precisa ser reativado na aba Actions.
