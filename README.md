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

### With Docker

```bash
docker build -t address-lookup-api .
docker run --rm -p 5010:8080 -e ASPNETCORE_ENVIRONMENT=Development address-lookup-api
```

The image uses the .NET 10 chiseled runtime: no shell, no package manager, running as a non-root user. Without `ASPNETCORE_ENVIRONMENT=Development`, the container runs in production mode, where CORS only allows the deployed front end. If the `PORT` environment variable is set (as hosting platforms like Render do), the API listens on it instead of 8080.

## Health and Limits

- `GET /health`: liveness. Says only whether the API process is up, without calling anything external.
- `GET /health/ready`: checks ViaCEP, BrasilAPI and IBGE. A source that is down makes the status `Degraded`, since the API keeps answering.
- Each IP can make 60 requests per minute. Above that, the API answers `429` with a `Retry-After` header. `/health` is not limited.
- In production, only the origins in `Cors:AllowedOrigins` and the Vercel preview links matched by `Cors:AllowedOriginPatterns` (`appsettings.json`) can call the API from a browser. In development, any origin can.

## Deployment

- **API:** [Render](https://render.com), free plan, described in [`render.yaml`](render.yaml). Every merge to `main` that touches the API is deployed only after the CI check passes, and Render waits for `/health` before switching traffic.
- **Front end:** Vercel, with a preview link for each pull request.
- The front end picks the API by its own address: opened from disk or `localhost`, it calls `http://localhost:5010`; deployed, it calls the Render URL (`src/js/config.js`).
- On the free plan the API sleeps after 15 minutes without traffic and takes up to a minute to wake up. The front end shows a notice when a response takes longer than 3 seconds.

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
