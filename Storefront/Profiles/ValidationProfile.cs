using AutoMapper;
using RpcValidationService;
using Storefront.Models.OcrService;

namespace Storefront.Profiles;

public class ValidationProfile: Profile
{
    public ValidationProfile()
    {
        CreateMap<RpcReceipt, Receipt>()
            .ForMember(dest => dest.CheckedAt, opt => opt.MapFrom(src => src.CheckedAt.ToDateTime()));
    }
}