using AutoMapper;
using DTOLayer.WebApiDTO.AppUserDTO;
using EntityLayer.Entities;

namespace WebApi.Mapping
{
    public class GeneralMapping : Profile
    {
        public GeneralMapping()
        {
            CreateMap<AppUser, UserListDto>().ReverseMap();
        }
    }
}
