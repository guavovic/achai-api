# Address Lookup API

A web application with a C#/.NET back-end that looks up Brazilian addresses by ZIP code (CEP) or by street, with a simple interface for querying the results.

## Tech Stack

C# / .NET 10, JavaScript, jQuery and CSS.

- **ViaCEP** for ZIP code and street lookups, with **BrasilAPI** as a fallback for ZIP codes when ViaCEP is down or slow.
- **IBGE** for the list of cities in each state.
- Timeout, retry and circuit breaker on every external call (`Microsoft.Extensions.Http.Resilience`), and an in-memory cache (`HybridCache`).

Architecture decisions are recorded in [`docs/decisions`](docs/decisions).

## How to Run

Requires the .NET 10 SDK.

1. Start the API:

   ```bash
   cd src/api
   dotnet run
   ```

   It listens on `http://localhost:5010`, and Swagger opens at `http://localhost:5010/swagger`.

2. Open `src/index.html` in the browser.

## How to Test

From the repository root:

```bash
dotnet test
```

Unit and integration tests use xUnit v3, NSubstitute and Shouldly. ViaCEP, IBGE and BrasilAPI are replaced by fake HTTP handlers, so the tests do not need network access.

## Project Structure

The API uses Vertical Slice Architecture with Minimal APIs:

```
src/api/
  Features/         one file per endpoint (route + handler)
    Addresses/      GetAddressByZipCode, SearchAddressesByStreet
    Cities/         GetCitiesByState
  Common/           Result, errors, validation, ProblemDetails
  Infrastructure/   ViaCEP, IBGE and BrasilAPI clients, cache and fallback decorators
tests/AddressLookup.Api.Tests/
  Unit/             handlers, clients and decorators
  Integration/      the whole API in memory (WebApplicationFactory)
```

## How to Use

**Search by ZIP code (CEP):** enter a valid ZIP code (e.g. 88350250) and click Search. The address is displayed in the right panel.

**Search by State and City:** select the state and the city, enter the street name (minimum 3 characters) and click Search. Results are listed dynamically.
