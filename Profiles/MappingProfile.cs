using AutoMapper;
using warehouse_api.Dtos;
using warehouse_api.Models;

namespace warehouse_api.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Resource, ResourceDto>();
        CreateMap<Unit, UnitDto>();
        CreateMap<ArrivalResource, ArrivalResourceDto>();

        CreateMap<ArrivalDto, Arrival>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToUniversalTime()));

                CreateMap<ArrivalResource, ArrivalResourceDto>();
        CreateMap<Arrival, ArrivalDto>();

        CreateMap<ResourceDto, Resource>();
        CreateMap<UnitDto, Unit>();
        CreateMap<ArrivalResourceDto, ArrivalResource>();
        CreateMap<ArrivalDto, Arrival>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToUniversalTime()));
    }
}