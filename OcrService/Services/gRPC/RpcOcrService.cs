using AutoMapper;
using Grpc.Core;
using OcrService.Models;
using RpcOcrService;
using static RpcOcrService.RpcOcrService;

namespace OcrService.Services.gRPC;

public class RpcOcrService: RpcOcrServiceBase
{
    private readonly PaddleService _paddleService;
    private readonly IMapper _mapper;

    public RpcOcrService(PaddleService paddleService, IMapper mapper)
    {
        _paddleService = paddleService;
        _mapper = mapper;
    }
    
    public override async Task<RpcReceipt> GetReceiptFromImage(RpcGetRecieptFromImageRequest request, ServerCallContext context)
    {
        Receipt receipt = await _paddleService.GetReceiptFromImageAsync(request.Base64String, context.CancellationToken);
        RpcReceipt result = _mapper.Map<RpcReceipt>(receipt);
        
        return result;
    }
}