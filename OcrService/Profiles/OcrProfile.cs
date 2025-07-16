using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using OcrService.Models;
using RpcOcrService;

namespace OcrService.Profiles;

public class OcrProfile: Profile
{
    public OcrProfile()
    {
        CreateMap<Receipt, RpcReceipt>()
            .ForMember(dest => dest.CheckedAt,
                opt => opt.MapFrom(src => Timestamp.FromDateTime(DateTime.SpecifyKind(src.CheckedAt, DateTimeKind.Utc))));
    }
}