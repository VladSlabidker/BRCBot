using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RpcOcrService;
using Storefront.Models.OcrService;

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
        string base64string = Convert.ToBase64String(image);

        RpcGetRecieptFromImageRequest request = new() { Base64String = base64string };
        RpcReceipt receipt = await _tesseractService.GetReceiptFromImageAsync(request);

        Receipt result = _mapper.Map<Receipt>(receipt);
        
        return Ok(result);
    }

}

