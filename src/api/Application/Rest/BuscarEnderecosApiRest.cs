using System.Net;
using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Rest
{
    public class BuscarEnderecosApiRest : IApi
    {
        public const string ViaCepClient = "ViaCep";
        public const string IbgeClient = "Ibge";

        private readonly IHttpClientFactory _httpClientFactory;

        public BuscarEnderecosApiRest(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<EnderecoModel>> BuscarEnderecoPorCEP(string cep)
        {
            var viaCep = _httpClientFactory.CreateClient(ViaCepClient);
            using var apiResponse = await viaCep.GetAsync($"{cep}/json");

            // O ViaCEP responde 400 com uma página HTML quando o CEP está fora do formato.
            if (apiResponse.StatusCode == HttpStatusCode.BadRequest)
                return EnderecoErrors.CepInvalido;

            apiResponse.EnsureSuccessStatusCode();

            var endereco = await apiResponse.Content.ReadFromJsonAsync<EnderecoModel>();

            // CEP no formato certo, mas que não existe, volta 200 com {"erro": "true"}.
            if (endereco is null || endereco.Erro is not null)
                return EnderecoErrors.CepNaoEncontrado;

            return endereco;
        }

        public async Task<Result<List<EnderecoModel>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro)
        {
            var viaCep = _httpClientFactory.CreateClient(ViaCepClient);
            using var apiResponse = await viaCep.GetAsync($"{uf}/{cidade}/{logradouro}/json");

            if (apiResponse.StatusCode == HttpStatusCode.BadRequest)
                return EnderecoErrors.BuscaInvalida;

            apiResponse.EnsureSuccessStatusCode();

            var enderecos = await apiResponse.Content.ReadFromJsonAsync<List<EnderecoModel>>();

            return enderecos ?? [];
        }

        public async Task<Result<List<CidadeModel>>> BuscarCidadesPorUF(string uf)
        {
            var ibge = _httpClientFactory.CreateClient(IbgeClient);
            using var apiResponse = await ibge.GetAsync($"localidades/estados/{uf}/municipios");

            apiResponse.EnsureSuccessStatusCode();

            var cidades = await apiResponse.Content.ReadFromJsonAsync<List<CidadeModel>>();

            // O IBGE devolve 200 com lista vazia para uma UF que não existe.
            if (cidades is null || cidades.Count == 0)
                return EnderecoErrors.UfNaoEncontrada;

            return cidades;
        }
    }
}
