using System.ComponentModel.DataAnnotations;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Validation;
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
        public async Task<IActionResult> BuscarEndereco(
            [FromRoute, RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP deve ter 8 dígitos, com ou sem traço.")] string cep)
        {
            var result = await _enderecoService.BuscarEnderecoPorCEP(cep);

            return result.Match(endereco => Ok(endereco), this.ToProblem);
        }

        [HttpGet("buscar/{uf}/{cidade}/{logradouro}")]
        public async Task<IActionResult> BuscarEndereco(
            [FromRoute, Uf] string uf,
            [FromRoute, MinLength(3, ErrorMessage = "A cidade precisa de pelo menos 3 caracteres.")] string cidade,
            [FromRoute, MinLength(3, ErrorMessage = "O logradouro precisa de pelo menos 3 caracteres.")] string logradouro)
        {
            var result = await _enderecoService.BuscarPorEstadoECidade(uf, cidade, logradouro);

            return result.Match(enderecos => Ok(enderecos), this.ToProblem);
        }

        [HttpGet("buscar/cidades/{uf}")]
        public async Task<IActionResult> BuscarCidades([FromRoute, Uf] string uf)
        {
            var result = await _enderecoService.BuscarCidadesPorUF(uf);

            return result.Match(cidades => Ok(cidades), this.ToProblem);
        }
    }
}
