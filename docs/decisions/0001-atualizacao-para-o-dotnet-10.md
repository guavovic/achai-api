# 1. Atualização para o .NET 10

Data: 30/09/2026

Status: Aceito

## Contexto

A API usava o .NET 9, uma versão de suporte padrão (STS) que deixa de ter suporte em 10 de novembro de 2026. Depois dessa data, ela não recebe mais correções de segurança.

O .NET 10 é uma versão de suporte longo (LTS), com suporte até novembro de 2028.

## Decisão

Mudar para `net10.0` e atualizar os pacotes que acompanham a versão do framework:

- `Microsoft.AspNetCore.OpenApi`, de 9.0.10 para 10.0.12
- `Swashbuckle.AspNetCore`, de 9.0.6 para 10.2.3

Na mesma mudança, tirar referências que não eram necessárias:

- `Newtonsoft.Json`, que nenhum arquivo usava
- `Swashbuckle.AspNetCore.Swagger`, `.SwaggerGen` e `.SwaggerUI`, que já vêm dentro do `Swashbuckle.AspNetCore`

O AutoMapper fica na 12.0.1 por enquanto. A troca dele tem uma decisão própria.

## Consequências

- O projeto roda num runtime com suporte por mais dois anos.
- Só o SDK do .NET 10 é necessário para compilar.
- O AutoMapper 12.0.1 continua com um alerta de vulnerabilidade de severidade alta (GHSA-rvv3-g6hj-g44x) até ser removido.
