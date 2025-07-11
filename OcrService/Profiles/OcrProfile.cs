using AutoMapper;
using OcrService.Models;
using RpcOcrService;

namespace OcrService.Profiles;

public class OcrProfile: Profile
{
    public OcrProfile()
    {
        CreateMap<RpcReceipt, Receipt>().ReverseMap();
    }
}