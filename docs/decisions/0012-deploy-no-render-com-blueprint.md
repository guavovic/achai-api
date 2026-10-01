# 12. Deploy da API no Render com Blueprint

Data: 30/09/2026

Status: Aceito

## Contexto

O front já é publicado na Vercel, mas chama `http://localhost:5010`, então o link não funciona para quem visita. A Vercel não roda .NET, e a API precisa de outra hospedagem. A imagem Docker (ADR 0011) e o CI (ADR 0004) já estão prontos. A ideia é ter uma demo pública de graça, sem cadastrar cartão de crédito.

## Opções consideradas

- **Render**: 750 horas grátis por mês (dá para o mês inteiro), 512 MB, sem cartão. Constrói direto do `Dockerfile` e aceita configuração versionada (`render.yaml`). Desliga depois de 15 minutos sem acesso e leva cerca de um minuto para voltar.
- **Koyeb**: um serviço grátis, desliga depois de uma hora sem acesso. Desde fevereiro de 2026 pede cartão e segura US$ 29 no cadastro.
- **Google Cloud Run**: franquia grande, escala sozinho e volta rápido. Exige cartão, e sem limite de gasto bem configurado pode gerar cobrança.
- **Azure Container Apps**: mesma franquia do Cloud Run, também com cartão, e a configuração mais trabalhosa das quatro.

Para o tempo de volta do Render, havia duas saídas: um serviço externo chamando a API a cada 14 minutos para ela não dormir, ou aceitar o tempo e avisar no front.

## Decisão

- **Render, plano grátis, região Virginia** (a mais próxima do Brasil entre as oferecidas).
- **Blueprint (`render.yaml`) na raiz**: a configuração da hospedagem fica no repositório e passa por PR, como o código.
- **Deploy só depois do CI verde** (`autoDeployTrigger: checksPass`): o Render espera o check `Build da API` passar no commit da `main` antes de publicar.
- **`/health` como health check**: o Render só manda tráfego para a versão nova depois que ela responde.
- **`buildFilter`**: só mudanças na API, no `Dockerfile`, no `.dockerignore` ou no `render.yaml` geram deploy. Mexer no front, nos testes ou na documentação não reconstrói a imagem.
- **Sem ping para manter a API acordada.** O front avisa quando a resposta demora mais de 3 segundos ("o servidor está acordando") e já chama o `/health` ao abrir a página, para a API começar a acordar enquanto a pessoa preenche o formulário.
- **O front escolhe a API pelo endereço da página**: aberto do disco ou de `localhost`, usa a API local; publicado, usa a do Render. Como o front é JavaScript sem etapa de build, uma variável de ambiente da Vercel não chegaria nele. O `config.js` virou a única fonte do endereço da API e da lista de UFs, que antes estavam duplicados no `main.js`.
- **CORS aceita os links de preview da Vercel** por padrão (`Cors:AllowedOriginPatterns`), além da origem fixa de produção. O padrão exige `https`, o nome do projeto e o sufixo da conta (`-guavovic-projects.vercel.app`), e vai ancorado no começo e no fim para não aceitar domínios parecidos.

## Consequências

- A demo fica pública, sem custo e sem cartão.
- A primeira visita depois de um tempo parado leva até um minuto. O aviso no front explica a espera, e os resultados seguintes vêm do cache (ADR 0007).
- O cache em memória some quando a API dorme ou é publicada de novo. Para uma demo, isso basta.
- Os previews da Vercel passam a funcionar de ponta a ponta, chamando a API publicada.
- Trocar de hospedagem depois é barato: a imagem é a mesma, e só o `render.yaml` e o endereço no `config.js` são do Render.
