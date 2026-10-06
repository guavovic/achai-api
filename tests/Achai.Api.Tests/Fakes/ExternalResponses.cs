namespace Achai.Api.Tests.Fakes;

public static class ExternalResponses
{
    public const string ViaCepPracaDaSe = """
        {
          "cep": "01001-000",
          "logradouro": "Praça da Sé",
          "complemento": "lado ímpar",
          "unidade": "",
          "bairro": "Sé",
          "localidade": "São Paulo",
          "uf": "SP",
          "estado": "São Paulo",
          "regiao": "Sudeste"
        }
        """;

    public const string ViaCepNotFound = """
        {
          "erro": "true"
        }
        """;

    public const string ViaCepBadRequestPage = "<!DOCTYPE HTML><html><head><title>ViaCEP 400</title></head></html>";

    public const string EmptyList = "[]";

    public const string ViaCepListWithPracaDaSe = $"[{ViaCepPracaDaSe}]";

    public const string IbgeAcreCities = """
        [
          { "nome": "Acrelândia" },
          { "nome": "Assis Brasil" }
        ]
        """;

    public const string BrasilApiPracaDaSe = """
        {
          "cep": "01001000",
          "state": "SP",
          "city": "São Paulo",
          "neighborhood": "Sé",
          "street": "Praça da Sé",
          "service": "open-cep"
        }
        """;

    public static string BrasilApiWithLocation(string latitude, string longitude) => $$"""
        {
          "cep": "01001000",
          "state": "SP",
          "city": "São Paulo",
          "neighborhood": "Sé",
          "street": "Praça da Sé",
          "service": "open-cep",
          "location": { "type": "Point", "coordinates": { "latitude": "{{latitude}}", "longitude": "{{longitude}}" } }
        }
        """;

    public const string BrasilApiNotFound = """
        {
          "name": "CepPromiseError",
          "message": "Todos os serviços de CEP retornaram erro.",
          "type": "service_error"
        }
        """;

    public const string BrasilApiBadRequest = """
        {
          "name": "CepPromiseError",
          "message": "CEP deve conter exatamente 8 caracteres.",
          "type": "validation_error"
        }
        """;
}
