using Common.Enums;
using Common.Exceptions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using RpcOcrService;
using Tesseract;
using ValidationService.Configs;
using ValidationService.Interfaces;
using static RpcOcrService.RpcOcrService;
using Enum = System.Enum;
using RpcReceipt = RpcValidationService.RpcReceipt;
namespace ValidationService.Services;

public class ValidationService: IValidationService
{
    private readonly RpcOcrServiceClient _ocrService;
    private readonly ProxyConfig _proxy;
    
    public ValidationService(RpcOcrServiceClient ocrService, IOptions<ProxyConfig> proxy)
    {
        _ocrService = ocrService;
        _proxy = proxy.Value;
    }
    
    public async Task<RpcReceipt> ValidateReceiptAsync(string base64String, CancellationToken cancellationToken)
    {
        RpcGetRecieptFromImageRequest request = new() { Base64String = base64String };
        
        var ocrData = await _ocrService.GetReceiptFromImageAsync(request, cancellationToken:  cancellationToken);
        
        RpcReceipt receipt = await FillReceiptDataAsync(ocrData);
        
        return receipt;
    }

    private async Task<RpcReceipt> FillReceiptDataAsync(RpcOcrService.RpcReceipt? ocrData)
    {
        if (ocrData == null)
            throw new TesseractException("No information from receipt was given");
        
        Console.WriteLine(($"OCR Data: {ocrData.Code}, {Enum.GetName((BankType)ocrData.BankId)}"));
        
        (bool, string) result;
        
        result = (BankType)ocrData.BankId == BankType.Privat24
            ?
            await CheckPrivatService.ValidateReceiptAsync((BankType)ocrData.BankId, ocrData.Code)
            :
            await CheckGovService.ValidateReceiptAsync((BankType)ocrData.BankId, ocrData.Code, _proxy);
            
        if(!result.Item1)
            throw new InvalidReceiptException("Receipt was not valid");

        RpcReceipt receipt = new()
        {
            Amount = ocrData.Amount,
            Code = ocrData.Code,
            CheckedAt = DateTime.UtcNow.ToTimestamp(),
            BankId = ocrData.BankId,
            IsValid = true,
            Link = result.Item2
        };
        
        return receipt;
    }
}