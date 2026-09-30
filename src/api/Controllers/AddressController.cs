using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BuscarEnderecos.API.Controllers
{
    [ApiController]
    public class EnderecoController : ControllerBase
    {
        public readonly IEnderecoService _enderecoService;

        public EnderecoController(IEnderecoService enderecoService)
        {
            _enderecoService = enderecoService;
        }

        [HttpGet("buscar/{cep}")]
        public async Task<IActionResult> BuscarEndereco([FromRoute] string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return this.ToProblem(EnderecoErrors.CepInvalido);

            var result = await _enderecoService.BuscarEnderecoPorCEP(cep);

            return result.Match(endereco => Ok(endereco), this.ToProblem);
        }

        [HttpGet("buscar/{uf}/{cidade}/{logradouro}")]
        public async Task<IActionResult> BuscarEndereco([FromRoute] string uf, [FromRoute] string cidade, [FromRoute] string logradouro)
        {
            if (string.IsNullOrWhiteSpace(uf)
            || string.IsNullOrWhiteSpace(cidade) || cidade.Length < 3
            || string.IsNullOrWhiteSpace(logradouro) || logradouro.Length < 3)
            {
                return this.ToProblem(EnderecoErrors.BuscaInvalida);
            }

            var result = await _enderecoService.BuscarPorEstadoECidade(uf, cidade, logradouro);

            return result.Match(enderecos => Ok(enderecos), this.ToProblem);
        }

        [HttpGet("buscar/cidades/{uf}")]
        public async Task<IActionResult> BuscarCidades(string uf)
        {
            if (string.IsNullOrWhiteSpace(uf))
                return this.ToProblem(EnderecoErrors.UfObrigatoria);

            var result = await _enderecoService.BuscarCidadesPorUF(uf);

            return result.Match(cidades => Ok(cidades), this.ToProblem);
        }
    }
}
