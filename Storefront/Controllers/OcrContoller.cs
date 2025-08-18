using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RpcOcrService;
using static RpcOcrService.RpcOcrService;
using Storefront.Models.OcrService;

namespace Storefront.Controllers;

[ApiController]
[Route("api/ocr")]
public class OcrContoller : ControllerBase
{
    private readonly RpcOcrServiceClient _ocrServiceClient;
    private readonly IMapper _mapper;
    
    public OcrContoller(RpcOcrServiceClient ocrServiceClient, IMapper mapper)
    {
        _ocrServiceClient = ocrServiceClient;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> GetReceipt([FromBody]byte[] image, CancellationToken cancellationToken)
    {
        string base64string = Convert.ToBase64String(image);

        RpcGetRecieptFromImageRequest request = new() { Base64String = base64string };
        RpcReceipt receipt = await _ocrServiceClient.GetReceiptFromImageAsync(request, cancellationToken: cancellationToken);

        Receipt result = _mapper.Map<Receipt>(receipt);
        
        return Ok(result);
    }

}

