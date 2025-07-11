using AutoMapper;
using OcrService.Models;
using RpcOcrService;

namespace Storefront.Profiles;

public class OcrProfile : Profile
{
    public OcrProfile()
    {
        CreateMap<Receipt, RpcReceipt>().ReverseMap();
    }
}
