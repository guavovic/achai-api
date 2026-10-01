using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Results;
using BuscarEnderecos.API.Services;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class EnderecoServiceTests
    {
        private readonly IApi _api = Substitute.For<IApi>();
        private readonly EnderecoService _service;

        public EnderecoServiceTests()
        {
            _service = new EnderecoService(_api);
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_TiraOTracoAntesDeConsultar()
        {
            _api.BuscarEnderecoPorCEP(Arg.Any<string>()).Returns(new EnderecoModel { CEP = "01001-000" });

            await _service.BuscarEnderecoPorCEP("01001-000");

            await _api.Received(1).BuscarEnderecoPorCEP("01001000");
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoAchou_MapeiaTodosOsCampos()
        {
            var model = new EnderecoModel
            {
                CEP = "01001-000",
                Logradouro = "Praça da Sé",
                Complemento = "lado ímpar",
                Unidade = "",
                Bairro = "Sé",
                Localidade = "São Paulo",
                UF = "SP",
                Estado = "São Paulo",
                Regiao = "Sudeste"
            };
            _api.BuscarEnderecoPorCEP("01001000").Returns(model);

            var result = await _service.BuscarEnderecoPorCEP("01001000");

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldSatisfyAllConditions(
                dto => dto.CEP.ShouldBe("01001-000"),
                dto => dto.Logradouro.ShouldBe("Praça da Sé"),
                dto => dto.Complemento.ShouldBe("lado ímpar"),
                dto => dto.Unidade.ShouldBe(""),
                dto => dto.Bairro.ShouldBe("Sé"),
                dto => dto.Localidade.ShouldBe("São Paulo"),
                dto => dto.UF.ShouldBe("SP"),
                dto => dto.Estado.ShouldBe("São Paulo"),
                dto => dto.Regiao.ShouldBe("Sudeste"));
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoAApiDevolveErro_RepassaOErro()
        {
            _api.BuscarEnderecoPorCEP(Arg.Any<string>()).Returns(EnderecoErrors.CepNaoEncontrado);

            var result = await _service.BuscarEnderecoPorCEP("99999999");

            result.Error.ShouldBe(EnderecoErrors.CepNaoEncontrado);
        }

        [Fact]
        public async Task BuscarPorEstadoECidade_MandaAUfEmMaiuscula()
        {
            _api.BuscarPorEstadoECidade(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new List<EnderecoModel>());

            await _service.BuscarPorEstadoECidade("sp", "São Paulo", "Paulista");

            await _api.Received(1).BuscarPorEstadoECidade("SP", "São Paulo", "Paulista");
        }

        [Fact]
        public async Task BuscarCidadesPorUF_MandaAUfEmMaiusculaEMapeiaOsNomes()
        {
            _api.BuscarCidadesPorUF("AC").Returns(new List<CidadeModel>
            {
                new() { Nome = "Acrelândia" },
                new() { Nome = "Assis Brasil" }
            });

            var result = await _service.BuscarCidadesPorUF("ac");

            result.Value.Select(c => c.Nome).ShouldBe(["Acrelândia", "Assis Brasil"]);
        }
    }
}
