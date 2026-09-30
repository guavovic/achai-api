# 4. GitHub Flow com main protegida e CI

Data: 30/09/2026

Status: Aceito

## Contexto

Até aqui toda mudança era commitada direto na `main`, sem nada conferindo se o código ainda compilava. O projeto tem uma pessoa só mantendo, faz deploy contínuo (o front está na Vercel) e não tem versões lançadas para dar suporte ao mesmo tempo.

## Opções consideradas

- **Git Flow**, com uma branch `develop` de vida longa, mais branches de release e hotfix. Feito para software entregue em versões e para times maiores. O próprio autor do modelo recomendou depois algo mais simples para aplicações web com entrega contínua.
- **GitHub Flow**: a `main` está sempre pronta para deploy, cada mudança vai numa branch curta e entra na `main` por pull request.
- **Trunk-based** com commit direto na `main`. Rápido, mas abre mão da revisão e da PR como registro de cada mudança.

## Decisão

GitHub Flow.

- Toda mudança vai numa branch própria (`feat/...`, `fix/...`, `ci/...`) e entra na `main` por pull request.
- A `main` é protegida: pull request obrigatória, e o check `Build da API` precisa passar antes do merge. A regra vale também para administradores.
- O workflow de CI (`.github/workflows/ci.yml`) faz o restore e o build da API em toda pull request e em todo push na `main`. Aviso de compilação quebra o build (`-warnaserror`), para problemas como variável sem uso ou aviso de nulidade não se acumularem.
- A Vercel já cria um deploy de preview para cada pull request, o que cobre o papel que um ambiente "dev" separado teria.
- Se um dia precisar de versões lançadas, elas serão tags na `main`, não uma branch `develop`.

## Consequências

- A `main` só recebe código que compila sem avisos.
- Cada mudança tem uma pull request explicando o que faz e como foi testada.
- Os testes entram no workflow conforme forem escritos.
- O preview da Vercel só funciona de ponta a ponta quando a API estiver publicada, porque o front ainda chama uma API local.
