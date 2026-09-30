# 2. Replace AutoMapper with manual mapping

Date: 2026-09-30

Status: Accepted

## Context

The API used AutoMapper 12.0.1 to convert the ViaCEP and IBGE models into the response DTOs. Two problems:

- Version 12.0.1 has a high severity advisory (GHSA-rvv3-g6hj-g44x), reported by `dotnet build`.
- Since version 15, AutoMapper is dual licensed (RPL-1.5 or commercial). Upgrading to a fixed version means taking on a license that a public portfolio project does not need.

The project has only two mappings, both one to one: address and city.

## Options considered

- **Manual mapping** with extension methods (`model.ToDto()`).
- **Mapperly**, which generates the mapping code at compile time.
- **Mapster**, a free runtime mapper with an API close to AutoMapper.

## Decision

Manual mapping, in `Application/Mapping/EnderecoMappings.cs`.

With two mappings, a library adds more than it saves. Manual code has no dependency, can be debugged line by line, and a missing property is visible in the code instead of being skipped silently at runtime.

That last point already showed up: `EnderecoResponseDTO` had `Complemento` and `Unidade`, but `EnderecoModel` did not, so both fields were always `null` and AutoMapper never complained. The model now reads both fields from ViaCEP.

## Consequences

- No known vulnerability left in the dependencies.
- A new field in a DTO needs a line in the mapping method. This is the intended trade-off.
- If the number of mappings grows a lot, Mapperly is the first alternative to revisit.
