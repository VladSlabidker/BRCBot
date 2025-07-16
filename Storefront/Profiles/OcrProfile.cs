using AutoMapper;
using Storefront.Models.OcrService;
using RpcOcrService;

namespace Storefront.Profiles;

public class OcrProfile : Profile
{
    public OcrProfile()
    {
        CreateMap<RpcReceipt, Receipt>()
            .ForMember(dest => dest.CheckedAt, opt => opt.MapFrom(src => src.CheckedAt.ToDateTime()));
    }
}
