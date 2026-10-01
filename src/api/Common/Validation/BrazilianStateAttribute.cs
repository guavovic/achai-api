using System.ComponentModel.DataAnnotations;

namespace AddressLookup.Api.Common.Validation;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class BrazilianStateAttribute : ValidationAttribute
{
    public BrazilianStateAttribute() : base("A UF informada não existe. Use uma das 27 siglas, como SP ou RJ.")
    {
    }

    public override bool IsValid(object? value) =>
        BrazilianStates.Exists(value as string);
}
