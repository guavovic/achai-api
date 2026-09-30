# Address Lookup API

A web application with a C#/.NET back-end that consumes the ViaCEP and BrasilAPI public APIs to retrieve address data, with a simple interface for querying the results.

## Tech Stack

C# / .NET, JavaScript, jQuery and CSS. Integrates the ViaCEP API and BrasilAPI.

## How to Run

Requires the .NET 9 SDK.

1. Start the API:

   ```bash
   cd src/api
   dotnet run
   ```

   It listens on `http://localhost:5010`, and Swagger opens at `http://localhost:5010/swagger`.

2. Open `src/index.html` in the browser.

## How to Use

**Search by ZIP code (CEP):** enter a valid ZIP code (e.g. 88350250) and click Search. The address is displayed in the right panel.

**Search by State and City:** select the state and the city, enter the street name (minimum 3 characters) and click Search. Results are listed dynamically.
