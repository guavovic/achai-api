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
    }
}
