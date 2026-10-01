# Política de segurança

## Versões com suporte

Só a versão publicada (a `main`, em [achai-api.onrender.com](https://achai-api.onrender.com/docs)) recebe correções.

## Como relatar uma vulnerabilidade

Não abra uma issue pública. Use o relato privado do GitHub: na aba **Security** deste repositório, clique em **Report a vulnerability**.

Inclua o que for possível: a rota, a requisição que mostra o problema e o que acontece. A API é pública e só faz consultas, sem login nem dados pessoais guardados. Os casos que interessam mais são, por exemplo, contornar o rate limit, fazer a API chamar endereços que não sejam o ViaCEP, a BrasilAPI ou o IBGE, ou expor detalhes internos nas respostas de erro.

Respondo em até 7 dias. Se a vulnerabilidade for confirmada, a correção vai para a `main` e é publicada, e o relato é creditado, se você quiser.

Dependências com vulnerabilidade conhecida são acompanhadas pelo Dependabot, que abre pull requests de correção automaticamente.
