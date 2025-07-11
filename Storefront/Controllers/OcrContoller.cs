using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using OcrService.Models;
using OcrService.Services.gRPC;
using RpcOcrService;
using static RpcOcrService.RpcOcrService.RpcOcrServiceClient;

namespace Storefront.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrContoller : ControllerBase
{
    private readonly RpcOcrService.RpcOcrService.RpcOcrServiceClient _tesseractService;
    private readonly IMapper _mapper;
    
    public OcrContoller(RpcOcrService.RpcOcrService.RpcOcrServiceClient tesseractService, IMapper mapper)
    {
        _tesseractService = tesseractService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> GetReceipt([FromBody]byte[] image)
    {
        var base64string = Convert.ToBase64String(image);

        RpcGetRecieptFromImageRequest request = new() { Base64String = base64string };
        var receipt = await _tesseractService.GetReceiptFromImageAsync(request);
        
        var result = new Receipt()
        {
            Amount = receipt.Amount,
            BankId = receipt.BankId,
            Code = receipt.Code,
            LastFour = receipt.LastFour
        };
        
        return Ok(result);
    }

}

