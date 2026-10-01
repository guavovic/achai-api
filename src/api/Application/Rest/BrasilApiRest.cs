using System.Net;
using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Rest
{
    public class BrasilApiRest : ICepReserva
    {
        public const string BrasilApiClient = "BrasilApi";

        private readonly IHttpClientFactory _httpClientFactory;

        public BrasilApiRest(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<EnderecoModel>> BuscarEnderecoPorCEP(string cep)
        {
            var brasilApi = _httpClientFactory.CreateClient(BrasilApiClient);
            using var apiResponse = await brasilApi.GetAsync($"cep/v2/{cep}");

            if (apiResponse.StatusCode == HttpStatusCode.BadRequest)
                return EnderecoErrors.CepInvalido;

            if (apiResponse.StatusCode == HttpStatusCode.NotFound)
                return EnderecoErrors.CepNaoEncontrado;

            apiResponse.EnsureSuccessStatusCode();

            var model = await apiResponse.Content.ReadFromJsonAsync<BrasilApiCepModel>();

            if (model is null)
                return EnderecoErrors.CepNaoEncontrado;

            return ParaEndereco(model);
        }

        // A BrasilAPI não traz complemento, unidade, nome do estado nem região.
        // Os dois últimos vêm da tabela de UFs, para a resposta ficar igual à do ViaCEP.
        private static EnderecoModel ParaEndereco(BrasilApiCepModel model)
        {
            var uf = UnidadesFederativas.Obter(model.State);

            return new EnderecoModel
            {
                CEP = FormatarCep(model.Cep),
                Logradouro = model.Street,
                Complemento = string.Empty,
                Unidade = string.Empty,
                Bairro = model.Neighborhood,
                Localidade = model.City,
                UF = model.State,
                Estado = uf?.Nome,
                Regiao = uf?.Regiao
            };
        }

        private static string? FormatarCep(string? cep) =>
            cep is { Length: 8 } ? $"{cep[..5]}-{cep[5..]}" : cep;
    }
}
