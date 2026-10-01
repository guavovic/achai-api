using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Caching
{
    /// <summary>
    /// Forma serializável de um <see cref="Result{T}"/>, para guardar no cache tanto o valor quanto o erro esperado.
    /// </summary>
    public sealed record ResultadoEmCache<T>(T? Valor, Error? Erro)
    {
        public static ResultadoEmCache<T> De(Result<T> result) =>
            result.Match(valor => new ResultadoEmCache<T>(valor, null), erro => new ResultadoEmCache<T>(default, erro));

        public Result<T> ParaResult() =>
            Erro is null ? Result<T>.Success(Valor!) : Result<T>.Failure(Erro);
    }
}
