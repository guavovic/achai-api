using System.ComponentModel.DataAnnotations;
using BuscarEnderecos.API.Models;

namespace BuscarEnderecos.API.Validation
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class UfAttribute : ValidationAttribute
    {
        public UfAttribute() : base("A UF informada não existe. Use uma das 27 siglas, como SP ou RJ.")
        {
        }

        public override bool IsValid(object? value) =>
            UnidadesFederativas.Existe(value as string);
    }
}
