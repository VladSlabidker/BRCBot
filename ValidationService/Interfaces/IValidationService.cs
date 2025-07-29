using Common.Enums;
using RpcValidationService;

namespace ValidationService.Interfaces;

public interface IValidationService
{
    public Task<RpcReceipt> ValidateReceiptAsync(string base64String, CancellationToken cancellationToken);
}