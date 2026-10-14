# 12. Deploy da API no Render com Blueprint

Data: 30/09/2026

Status: Aceito

## Contexto

O front já é publicado na Vercel, mas chama `http://localhost:5010`, então o link não funciona para quem visita. A Vercel não roda .NET, e a API precisa de outra hospedagem. A imagem Docker (ADR 0011) e o CI (ADR 0004) já estão prontos. A ideia é ter uma demo pública de graça, sem cadastrar cartão de crédito.

## Opções consideradas

- **Render**: sem cartão. Constrói direto do `Dockerfile` e aceita configuração versionada (`render.yaml`). Depois de um tempo sem acesso, a primeira resposta leva cerca de um minuto.
- **Koyeb**: desde fevereiro de 2026 pede cartão e segura US$ 29 no cadastro.
- **Google Cloud Run**: franquia grande, escala sozinho e volta rápido. Exige cartão, e sem limite de gasto bem configurado pode gerar cobrança.
- **Azure Container Apps**: mesma franquia do Cloud Run, também com cartão, e a configuração mais trabalhosa das quatro.

Para essa primeira resposta lenta, havia duas saídas: um serviço externo chamando a API de tempos em tempos para mantê-la ativa, ou aceitar o tempo e avisar no front.

## Decisão

- **Render, região Virginia** (a mais próxima do Brasil entre as oferecidas).
- **Blueprint (`render.yaml`) na raiz**: a configuração da hospedagem fica no repositório e passa por PR, como o código.
- **Deploy só depois do CI verde**: a hospedagem espera o check `Build da API` passar no commit da `main` antes de publicar.
- **`/health` como health check**: a hospedagem só manda tráfego para a versão nova depois que ela responde.
- **Deploy só quando importa**: só mudanças na API, no `Dockerfile`, no `.dockerignore` ou no `render.yaml` geram deploy. Mexer no front, nos testes ou na documentação não reconstrói a imagem.
- **Sem ping para manter a API ativa.** O front avisa quando a resposta demora mais de 3 segundos e já chama o `/health` ao abrir a página, para a API começar a responder enquanto a pessoa preenche o formulário.
- **O front escolhe a API pelo endereço da página**: aberto do disco ou de `localhost`, usa a API local; publicado, usa a API publicada. Como o front é JavaScript sem etapa de build, uma variável de ambiente da Vercel não chegaria nele. Um único arquivo de configuração virou a fonte do endereço da API e da lista de UFs, que antes estavam duplicados.
- **CORS aceita os links de preview da Vercel**, além da origem fixa de produção, com um padrão restrito ao projeto para não aceitar domínios parecidos.

## Consequências

- A demo fica pública, sem custo e sem cartão.
- A primeira visita depois de um tempo parado leva até um minuto. O aviso no front explica a espera, e os resultados seguintes vêm do cache (ADR 0007).
- O cache em memória some quando a API reinicia ou é publicada de novo. Para uma demo, isso basta.
- Os previews da Vercel passam a funcionar de ponta a ponta, chamando a API publicada.
- Trocar de hospedagem depois é barato: a imagem é a mesma, e só o `render.yaml` e o endereço da API no front são do Render.
