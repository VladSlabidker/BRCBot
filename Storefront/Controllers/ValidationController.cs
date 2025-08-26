using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RpcValidationService;
using Storefront.Models.OcrService;
using static RpcValidationService.RpcValidationService;

namespace Storefront.Controllers;

[ApiController]
[Route("api/validation")]
public class ValidationController : ControllerBase
{
    private readonly ILogger<ValidationController> _logger;
    private readonly RpcValidationServiceClient _validationServiceClient;
    private readonly IMapper _mapper;
    
    public ValidationController(ILogger<ValidationController> logger, IMapper mapper, RpcValidationServiceClient validationServiceClient)
    {
        _logger  = logger;
        _mapper =  mapper;
        _validationServiceClient = validationServiceClient;
    }

    [HttpPost]
    public async Task<IActionResult> GetReceipt([FromBody]byte[] image, CancellationToken cancellationToken)
    {
        string base64string = Convert.ToBase64String(image);

        RpcValidateReceiptRequest validateReceiptRequest = new RpcValidateReceiptRequest()
        {
            Base64String = base64string
        };
        
        var receipt = await _validationServiceClient.ValidateReceiptAsync(validateReceiptRequest, cancellationToken: cancellationToken);
        
        Receipt result = _mapper.Map<Receipt>(receipt);
        
        return Ok(receipt);
    }

}
