using System.Dynamic;
using System.Net;
using System.Text.Json;
using BuscarEnderecos.API.DTOs;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Settings;

namespace BuscarEnderecos.API.Rest
{
    public class BuscarEnderecosApiRest : IApi
    {
        private readonly HttpClient _viacephttpClient;
        private readonly HttpClient _ibgeHttpClient;

        public BuscarEnderecosApiRest()
        {
            _viacephttpClient = new HttpClient { BaseAddress = new Uri(ApiUrls.VIA_CEP) };
            _ibgeHttpClient = new HttpClient { BaseAddress = new Uri(ApiUrls.IBGE) };
        }

        public async Task<ResponseDTO<EnderecoModel>> BuscarEnderecoPorCEP(string cep)
        {
            var response = new ResponseDTO<EnderecoModel>();

            try
            {
                var apiResponse = await _viacephttpClient.GetAsync($"{cep}/json");
                var contentResponse = await apiResponse.Content.ReadAsStringAsync();

                if (apiResponse.IsSuccessStatusCode)
                {
                    response.ResponseData = JsonSerializer.Deserialize<EnderecoModel>(contentResponse);
                }
                else
                {
                    response.ResponseError = JsonSerializer.Deserialize<ExpandoObject>(contentResponse);
                }

                response.HttpCode = apiResponse.StatusCode;
            }
            catch (Exception ex)
            {

            }

            return response;
        }

        public async Task<ResponseDTO<List<EnderecoModel>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro)
        {
            var response = new ResponseDTO<List<EnderecoModel>>();

            try
            {
                var apiResponse = await _viacephttpClient.GetAsync($"{uf}/{cidade}/{logradouro}/json");
                var contentResponse = await apiResponse.Content.ReadAsStringAsync();

                if (apiResponse.IsSuccessStatusCode)
                {
                    response.ResponseData = JsonSerializer.Deserialize<List<EnderecoModel>>(contentResponse);
                }
                else
                {
                    response.ResponseError = JsonSerializer.Deserialize<ExpandoObject>(contentResponse);
                }

                response.HttpCode = apiResponse.StatusCode;
            }
            catch (Exception ex)
            {
                response.HttpCode = HttpStatusCode.InternalServerError;

            }

            return response;
        }

        public async Task<ResponseDTO<List<CidadeModel>>> BuscarCidadesPorUF(string uf)
        {
            var response = new ResponseDTO<List<CidadeModel>>();

            try
            {
                var apiResponse = await _ibgeHttpClient.GetAsync($"localidades/estados/{uf}/municipios");
                var contentResponse = await apiResponse.Content.ReadAsStringAsync();

                if (apiResponse.IsSuccessStatusCode)
                {
                    response.ResponseData = JsonSerializer.Deserialize<List<CidadeModel>>(contentResponse);
                }
                else
                {
                    response.ResponseError = JsonSerializer.Deserialize<ExpandoObject>(contentResponse);
                }

                response.HttpCode = apiResponse.StatusCode;
            }
            catch (Exception ex)
            {
                // response.HttpCode = HttpStatusCode.InternalServerError;
                // resp onse.ResponseError = ex.Message; // arrumar isso
            }

            return response;
        }
    }
}
