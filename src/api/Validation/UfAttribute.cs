using System.ComponentModel.DataAnnotations;

namespace BuscarEnderecos.API.Validation
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class UfAttribute : ValidationAttribute
    {
        private static readonly HashSet<string> Ufs = new(StringComparer.OrdinalIgnoreCase)
        {
            "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
            "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
        };

        public UfAttribute() : base("A UF informada não existe. Use uma das 27 siglas, como SP ou RJ.")
        {
        }

        public override bool IsValid(object? value) =>
            value is string uf && Ufs.Contains(uf);
    }
}
