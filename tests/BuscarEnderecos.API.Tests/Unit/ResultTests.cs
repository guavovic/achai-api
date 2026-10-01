using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class ResultTests
    {
        private static readonly Error ErroQualquer = Error.NotFound("Teste.NaoEncontrado", "Não encontrado.");

        [Fact]
        public void Success_GuardaOValor()
        {
            Result<int> result = 42;

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBe(42);
            result.Error.ShouldBeNull();
        }

        [Fact]
        public void Failure_GuardaOErro()
        {
            Result<int> result = ErroQualquer;

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe(ErroQualquer);
        }

        [Fact]
        public void Value_QuandoFalhou_LancaExcecao()
        {
            Result<int> result = ErroQualquer;

            Should.Throw<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void Map_QuandoDeuCerto_TransformaOValor()
        {
            Result<int> result = 21;

            var mapeado = result.Map(valor => valor * 2);

            mapeado.Value.ShouldBe(42);
        }

        [Fact]
        public void Map_QuandoFalhou_RepassaOErroSemChamarAFuncao()
        {
            Result<int> result = ErroQualquer;
            var chamou = false;

            var mapeado = result.Map(valor =>
            {
                chamou = true;
                return valor * 2;
            });

            chamou.ShouldBeFalse();
            mapeado.Error.ShouldBe(ErroQualquer);
        }

        [Fact]
        public void Match_ChamaOCaminhoCerto()
        {
            Result<int> sucesso = 1;
            Result<int> falha = ErroQualquer;

            sucesso.Match(_ => "sucesso", _ => "falha").ShouldBe("sucesso");
            falha.Match(_ => "sucesso", erro => erro.Code).ShouldBe("Teste.NaoEncontrado");
        }
    }
}
