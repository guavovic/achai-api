namespace BuscarEnderecos.API.Tests.Fakes
{
    /// <summary>
    /// Respostas reais do ViaCEP e do IBGE, copiadas das APIs, para os testes não dependerem da rede.
    /// </summary>
    public static class RespostasExternas
    {
        public const string EnderecoPracaDaSe = """
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

        public const string CepInexistente = """
            {
              "erro": "true"
            }
            """;

        public const string PaginaDeErro400 = "<!DOCTYPE HTML><html><head><title>ViaCEP 400</title></head></html>";

        public const string ListaVazia = "[]";

        public const string ListaComUmEndereco = $"[{EnderecoPracaDaSe}]";

        public const string CidadesDoAcre = """
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

        public const string BrasilApiCepNaoEncontrado = """
            {
              "name": "CepPromiseError",
              "message": "Todos os serviços de CEP retornaram erro.",
              "type": "service_error"
            }
            """;

        public const string BrasilApiCepInvalido = """
            {
              "name": "CepPromiseError",
              "message": "CEP deve conter exatamente 8 caracteres.",
              "type": "validation_error"
            }
            """;
    }
}
