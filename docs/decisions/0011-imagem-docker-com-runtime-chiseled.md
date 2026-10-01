# 11. Imagem Docker com runtime chiseled

Data: 30/09/2026

Status: Aceito

## Contexto

A API vai ser publicada, e a opção mais provável, o Render, não tem runtime nativo de .NET: ele roda containers. Uma imagem Docker também deixa a API rodar igual em qualquer máquina ou hospedagem.

## Opções consideradas

- **Como montar a imagem:**
  - **Dockerfile multi-stage**: um estágio compila com a imagem do SDK, e o final copia só o resultado para a imagem de runtime. Explícito, e o Render constrói direto do `Dockerfile` do repositório.
  - **`dotnet publish /t:PublishContainer`**: o próprio SDK gera a imagem, sem `Dockerfile`. Moderno, mas o Render precisaria de uma imagem já publicada num registry, o que exigiria mais um passo no CI.
- **Imagem base de runtime:**
  - `aspnet:10.0`, a padrão, com shell e pacotes do sistema.
  - `aspnet:10.0-alpine`, menor, mas com a biblioteca C musl, que às vezes se comporta diferente.
  - `aspnet:10.0-noble-chiseled`: Ubuntu reduzido ao que o .NET precisa, sem shell, sem gerenciador de pacotes e rodando sem root. É a recomendação atual da Microsoft para produção.

## Decisão

- **Dockerfile multi-stage na raiz**, com o SDK no build e o runtime **chiseled** no estágio final.
- O `csproj` é copiado e restaurado antes do código, para o Docker reaproveitar a camada dos pacotes quando só o código muda.
- **`.dockerignore` como lista de permissão**: só `src/api/` entra no build, sem `bin/`, `obj/` e arquivos locais.
- A imagem roda com o usuário sem privilégio das imagens .NET e escuta na porta 8080, o padrão desde o .NET 8.
- **A API lê a variável `PORT`**, que hospedagens como o Render usam para dizer em que porta escutar. Como a imagem chiseled não tem shell, isso não dá para resolver no `ENTRYPOINT`, então fica no `Program.cs`.
- **Sem `HEALTHCHECK` no Dockerfile**: a imagem não tem `curl`, e quem confere a saúde é a hospedagem, pelo `/health`.
- **Sem `docker-compose`**: é um serviço só.
- **O CI constrói a imagem** em toda PR, só para garantir que o `Dockerfile` não quebrou. Nada é publicado.

## Consequências

- Imagem final de 54 MB comprimida (185 MB descompactada). Só a base `aspnet:10.0` padrão, sem a API, já tem 340 MB descompactada.
- Conferido rodando: o container responde na 8080 e, com `PORT=10000`, na 10000; roda como o usuário 1654; e `docker exec ... sh` falha, porque não há shell.
- Sem shell, não dá para entrar no container para investigar um problema. Os logs (`docker logs`) e o `/health/ready` passam a ser as ferramentas para isso.
- Rodando o container localmente, ele fica em modo produção por padrão, e o CORS só libera o front publicado. Para usar com o `index.html` local, é preciso passar `ASPNETCORE_ENVIRONMENT=Development`, como o README mostra.
