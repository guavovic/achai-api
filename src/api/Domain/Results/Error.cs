namespace BuscarEnderecos.API.Results
{
    public enum ErrorType
    {
        Validation,
        NotFound
    }

    public sealed record Error(string Code, string Description, ErrorType Type)
    {
        public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

        public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    }
}
