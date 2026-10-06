# 22. Servidor MCP dentro da API

Data: 06/10/2026

Status: Aceito

## Contexto

Assistentes de IA (Claude, Cursor e outros) usam ferramentas pelo protocolo MCP. Expor as buscas da achaí-API como ferramentas deixa qualquer assistente consultar endereços sem escrever código de integração, e quase nenhuma API de CEP brasileira oferece isso.

## Opções consideradas

- **Dentro da própria API, em `/mcp`**: reaproveita os provedores, o cache, o fallback e o limite de requisições; nada novo para hospedar.
- **Processo separado (stdio), publicado como pacote**: roda na máquina de quem usa, mas precisa de instalação e de chamar a API pela rede de qualquer jeito.
- **Serviço separado no Render**: isola o MCP, mas duplica a hospedagem e dorme do mesmo jeito.

## Decisão

- **Dentro da API**, com o SDK oficial (`ModelContextProtocol.AspNetCore`), transporte HTTP sem estado, em `/mcp`.
- Ferramentas: `buscar_cep`, `buscar_logradouro` (com as mesmas variações da busca tolerante), `listar_cidades` e `distancia_entre_ceps`, todas marcadas como só leitura.
- Erros de entrada (CEP fora do formato, UF que não existe) voltam como erro legível da ferramenta, sem chamar as fontes.

## Consequências

- Como as outras rotas, o `/mcp` passa pelo limite de 60 requisições por minuto por IP e dorme junto com o servidor.
- O SDK do MCP é uma dependência nova, que muda rápido; o Dependabot acompanha as versões.
