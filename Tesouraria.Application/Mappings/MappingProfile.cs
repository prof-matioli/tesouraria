using AutoMapper;
using Tesouraria.Application.DTOs;
using Tesouraria.Domain.Entities;

namespace Tesouraria.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeamento bidirecional (Domain <-> DTO)
            CreateMap<Fiel, FielDTO>().ReverseMap();
            CreateMap<Fornecedor, FornecedorDTO>().ReverseMap();
            CreateMap<CentroCusto, CentroCustoDTO>().ReverseMap();
            CreateMap<CategoriaFinanceira, CategoriaFinanceiraDTO>().ReverseMap();

            // Apenas de Usuario -> DTO (não precisa do ReverseMap para login)
            CreateMap<Usuario, UsuarioDTO>();
        }
    }
}