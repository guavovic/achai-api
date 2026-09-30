# 1. Upgrade to .NET 10

Date: 2026-09-30

Status: Accepted

## Context

The API targeted .NET 9, a Standard Term Support release that reaches end of support on November 10, 2026. After that date it receives no security fixes.

.NET 10 is a Long Term Support release, supported until November 2028.

## Decision

Target `net10.0` and update the packages that follow the framework version:

- `Microsoft.AspNetCore.OpenApi` from 9.0.10 to 10.0.12
- `Swashbuckle.AspNetCore` from 9.0.6 to 10.2.3

In the same change, remove references that were not needed:

- `Newtonsoft.Json`, not used by any source file
- `Swashbuckle.AspNetCore.Swagger`, `.SwaggerGen` and `.SwaggerUI`, already included by `Swashbuckle.AspNetCore`

AutoMapper stays at 12.0.1 for now. It is replaced in its own decision.

## Consequences

- The project runs on a supported runtime for two more years.
- Only the .NET 10 SDK is needed to build it.
- AutoMapper 12.0.1 still carries a high severity advisory (GHSA-rvv3-g6hj-g44x) until it is removed.
