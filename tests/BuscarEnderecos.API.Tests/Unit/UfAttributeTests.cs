using BuscarEnderecos.API.Validation;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class UfAttributeTests
    {
        private readonly UfAttribute _atributo = new();

        [Theory]
        [InlineData("SP")]
        [InlineData("sp")]
        [InlineData("Rj")]
        [InlineData("DF")]
        [InlineData("TO")]
        public void IsValid_ComSiglaExistente_Aceita(string uf)
        {
            _atributo.IsValid(uf).ShouldBeTrue();
        }

        [Theory]
        [InlineData("XX")]
        [InlineData("SPP")]
        [InlineData("S")]
        [InlineData("")]
        [InlineData(" SP")]
        public void IsValid_ComSiglaInexistente_Recusa(string uf)
        {
            _atributo.IsValid(uf).ShouldBeFalse();
        }

        [Fact]
        public void IsValid_ComNulo_Recusa()
        {
            _atributo.IsValid(null).ShouldBeFalse();
        }
    }
}
