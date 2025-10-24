using System.Net;
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
                return BadRequest("O Cep ta invalido");

            var response = await _enderecoService.BuscarEnderecoPorCEP(cep);

            if (response.HttpCode == HttpStatusCode.OK)
                return Ok(response.ResponseData);

            if (response.HttpCode == HttpStatusCode.NotFound)
                return NotFound(new { message = "endereço não encontrado"});

            return StatusCode((int)response.HttpCode, response.ResponseError);
        }
        
        [HttpGet("buscar/{uf}/{cidade}/{logradouro}")]
        public async Task<IActionResult> BuscarEndereco([FromRoute] string uf, [FromRoute] string cidade, [FromRoute] string logradouro)
        {
            if (string.IsNullOrWhiteSpace(uf) || string.IsNullOrWhiteSpace(cidade))
                return BadRequest("UF ou cidade invalidos");

            if (string.IsNullOrWhiteSpace(uf) 
            || string.IsNullOrWhiteSpace(cidade) || cidade.Length < 3
            || string.IsNullOrWhiteSpace(logradouro) || logradouro.Length < 3)
            {
                return BadRequest("UF, cidade e logradouro são obrigatórios e devem ter pelo menos 3 caracteres");
            }

            var response = await _enderecoService.BuscarPorEstadoECidade(uf, cidade, logradouro);

            if (response.HttpCode == HttpStatusCode.OK)
                return Ok(response.ResponseData);

            if (response.HttpCode == HttpStatusCode.NotFound)
                return NotFound(new { message = "Endereço não encontrado" });

            return StatusCode((int)response.HttpCode, response.ResponseError);
        }

        [HttpGet("buscar/cidades/{uf}")]
        public async Task<IActionResult> BuscarCidades(string uf)
        {
            if (string.IsNullOrWhiteSpace(uf))
                return BadRequest("UF é obrigatório");

            var response = await _enderecoService.BuscarCidadesPorUF(uf);

            if (response.HttpCode == HttpStatusCode.OK)
                return Ok(response.ResponseData);

            if (response.HttpCode == HttpStatusCode.NotFound)
                return NotFound(new { message = "Estado não encontrado"});

            return StatusCode((int)response.HttpCode, response.ResponseError);
        }
    }
}