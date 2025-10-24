using AutoMapper;
using BuscarEnderecos.API.DTOs;
using BuscarEnderecos.API.Models;

namespace BuscarEnderecos.API.Mapping
{
    public class AddressMapping : Profile
    {
        public AddressMapping()
        {
            CreateMap(typeof(ResponseDTO<>), typeof(ResponseDTO<>));

            CreateMap<EnderecoResponseDTO, EnderecoModel>();
            CreateMap<EnderecoModel, EnderecoResponseDTO>();
            
            CreateMap<CidadeResponseDTO, CidadeModel>();
            CreateMap<CidadeModel, CidadeResponseDTO>();
        }
    }
}