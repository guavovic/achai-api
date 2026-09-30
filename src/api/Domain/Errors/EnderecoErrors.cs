using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Errors
{
    public static class EnderecoErrors
    {
        public static readonly Error CepInvalido =
            Error.Validation("Endereco.CepInvalido", "O CEP informado é inválido. Use 8 dígitos, sem traço.");

        public static readonly Error CepNaoEncontrado =
            Error.NotFound("Endereco.CepNaoEncontrado", "Nenhum endereço encontrado para o CEP informado.");

        public static readonly Error BuscaInvalida =
            Error.Validation("Endereco.BuscaInvalida", "UF, cidade e logradouro são obrigatórios. Cidade e logradouro precisam de pelo menos 3 caracteres.");

        public static readonly Error UfObrigatoria =
            Error.Validation("Cidade.UfObrigatoria", "A UF é obrigatória.");

        public static readonly Error UfNaoEncontrada =
            Error.NotFound("Cidade.UfNaoEncontrada", "Nenhuma cidade encontrada para a UF informada.");
    }
}
