using AutoMapper;
using Grpc.Core;
using OcrService.Models;
using RpcOcrService;
using static RpcOcrService.RpcOcrService;

namespace OcrService.Services.gRPC;

public class RpcTesseractService: RpcOcrServiceBase
{
    private readonly Services.TesseractService _tesseractService;
    private readonly IMapper _mapper;

    public RpcTesseractService(Services.TesseractService tesseractService, IMapper mapper)
    {
        _tesseractService = tesseractService;
        _mapper = mapper;
    }
    
    public override async Task<RpcReceipt> GetReceiptFromImage(RpcGetRecieptFromImageRequest request, ServerCallContext context)
    {
        //TODO: Fix Mapper
        Receipt receipt = await _tesseractService.GetReceiptFromImageAsync(request.Base64String, context.CancellationToken);
        RpcReceipt result = _mapper.Map<RpcReceipt>(receipt);
        
        return result;
    }
}