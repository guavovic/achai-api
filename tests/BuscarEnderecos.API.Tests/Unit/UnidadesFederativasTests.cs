using BuscarEnderecos.API.Models;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class UnidadesFederativasTests
    {
        [Theory]
        [InlineData("SP", "São Paulo", "Sudeste")]
        [InlineData("df", "Distrito Federal", "Centro-Oeste")]
        [InlineData("AM", "Amazonas", "Norte")]
        [InlineData("RS", "Rio Grande do Sul", "Sul")]
        [InlineData("BA", "Bahia", "Nordeste")]
        public void Obter_DevolveNomeERegiao(string sigla, string nome, string regiao)
        {
            var uf = UnidadesFederativas.Obter(sigla);

            uf.ShouldNotBeNull();
            uf.Nome.ShouldBe(nome);
            uf.Regiao.ShouldBe(regiao);
        }

        [Fact]
        public void Obter_ComSiglaInexistente_DevolveNulo()
        {
            UnidadesFederativas.Obter("XX").ShouldBeNull();
        }
    }
}
