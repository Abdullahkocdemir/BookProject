using AutoMapper;
using DTOLayer.WebApiDTO.AppUserDTO;
using DTOLayer.WebApiDTO.ProductDTO;
using EntityLayer.Entities;
using System; // DateTime için

namespace WebApi.Mapping
{
    public class GeneralMapping : Profile
    {
        public GeneralMapping()
        {
            // AppUser Mapping'leri
            CreateMap<AppUser, UserListDto>().ReverseMap();
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // --- Product Mapping'leri ---
            CreateMap<ResultProductDTO, Product>().ReverseMap();
            CreateMap<GetByIdProductDTO, Product>().ReverseMap();

            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.PopulerProduct, opt => opt.MapFrom(src => false)) // Ensure this is mapped correctly from DTO
                .ForMember(dest => dest.ImageURl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());


            // UpdateProductDTO'dan Product'a mapping
            CreateMap<UpdateProductDTO, Product>()
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Now)) // Otomatik olarak güncellenme tarihi
                .ForMember(dest => dest.ImageURl, opt => opt.MapFrom(src => src.ImageUrl)) // ImageUrl yazım farkını eşle
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.PopulerProduct, opt => opt.MapFrom(src => src.PopulerProduct)) // <-- Hata Düzeltildi
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Oluşturulma tarihini güncellemede yok say
                .ForMember(dest => dest.Status, opt => opt.Ignore()); // Durumu güncellemede yok say (eğer ayrı bir metotla yönetiliyorsa)
        }
    }
}