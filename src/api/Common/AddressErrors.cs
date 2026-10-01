using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Common;

// Os códigos ficam em português porque fazem parte do contrato da API (campo "code" do ProblemDetails).
public static class AddressErrors
{
    public static readonly Error InvalidZipCode =
        Error.Validation("Endereco.CepInvalido", "O CEP informado é inválido. Use 8 dígitos, com ou sem traço.");

    public static readonly Error ZipCodeNotFound =
        Error.NotFound("Endereco.CepNaoEncontrado", "Nenhum endereço encontrado para o CEP informado.");

    public static readonly Error InvalidStreetSearch =
        Error.Validation("Endereco.BuscaInvalida", "UF, cidade e logradouro são obrigatórios. Cidade e logradouro precisam de pelo menos 3 caracteres.");

    public static readonly Error StateNotFound =
        Error.NotFound("Cidade.UfNaoEncontrada", "Nenhuma cidade encontrada para a UF informada.");
}
