using Api_c_sharp.DTO;
using Api_c_sharp.Models;
using AutoMapper;

namespace Api_c_sharp.Mapper;

public class MapperProfile: Profile
{
    public MapperProfile()
    {
        CreateMap<Adresse, AdresseDTO>()
            .ForMember(dest => dest.LibelleVille, opt => opt.MapFrom(src => src.VilleAdresseNav.Libelle))
            .ForMember(dest => dest.CodePostal, opt => opt.MapFrom(src => src.VilleAdresseNav.CodePostal))
            .ReverseMap();
    }
}