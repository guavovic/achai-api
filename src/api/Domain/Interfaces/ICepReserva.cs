using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Interfaces
{
    /// <summary>
    /// Fonte de CEP usada quando o ViaCEP falha.
    /// </summary>
    public interface ICepReserva
    {
        Task<Result<EnderecoModel>> BuscarEnderecoPorCEP(string cep);
    }
}
