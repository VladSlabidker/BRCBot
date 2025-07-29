using AutoMapper;
using Data.SQL.Models;
using Google.Protobuf.WellKnownTypes;
using RpcValidationService;

namespace ValidationService.Profiles;

public class ReceiptProfile: Profile
{
    public ReceiptProfile()
    {
        CreateMap<Receipt, RpcReceipt>()
            .ForMember(dest => dest.CheckedAt,
                opt => opt.MapFrom(src => src.CheckedAt.ToTimestamp()));
        
        CreateMap<RpcReceipt, Receipt>()
            .ForMember(dest => dest.CheckedAt,
                opt => opt.MapFrom(src => src.CheckedAt.ToDateTime()));;
    }
}