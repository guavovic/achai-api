# 2. Mapeamento manual no lugar do AutoMapper

Data: 30/09/2026

Status: Aceito

## Contexto

A API usava o AutoMapper 12.0.1 para converter os modelos do ViaCEP e do IBGE nos DTOs de resposta. Dois problemas:

- A versão 12.0.1 tem um alerta de vulnerabilidade de severidade alta (GHSA-rvv3-g6hj-g44x), apontado pelo `dotnet build`.
- Desde a versão 15, o AutoMapper tem licença dupla (RPL-1.5 ou comercial). Atualizar para uma versão corrigida significaria assumir uma licença de que um projeto público de portfólio não precisa.

O projeto tem só dois mapeamentos, os dois de um para um: endereço e cidade.

## Opções consideradas

- **Mapeamento manual**, com métodos de extensão (`model.ToDto()`).
- **Mapperly**, que gera o código do mapeamento na compilação.
- **Mapster**, um mapeador gratuito em tempo de execução, com uma API parecida com a do AutoMapper.

## Decisão

Mapeamento manual, em `Application/Mapping/EnderecoMappings.cs`.

Com dois mapeamentos, uma biblioteca acrescenta mais do que economiza. O código manual não tem dependência, pode ser depurado linha a linha, e uma propriedade esquecida aparece no código, em vez de ser ignorada em silêncio na execução.

Esse último ponto já apareceu: o `EnderecoResponseDTO` tinha `Complemento` e `Unidade`, mas o `EnderecoModel` não, então os dois campos voltavam sempre `null` e o AutoMapper nunca reclamou. O modelo agora lê os dois campos do ViaCEP.

## Consequências

- Nenhuma vulnerabilidade conhecida nas dependências.
- Um campo novo num DTO precisa de uma linha no método de mapeamento. É a troca pretendida.
- Se o número de mapeamentos crescer muito, o Mapperly é a primeira alternativa a reavaliar.
