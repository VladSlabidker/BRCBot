using AutoMapper;
using Data.Cache.Interfaces;
using Data.SQL;
using Data.SQL.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using RpcValidationService;
using static RpcValidationService.RpcValidationService;
using RpcReceipt = RpcValidationService.RpcReceipt;

namespace ValidationService.Services.gRPC;

public class RpcValidationService : RpcValidationServiceBase
{
    private readonly ILogger<RpcValidationService> _logger;
    private readonly BotContext _dbContext;
    private readonly ICacheClient _cacheClient;
    private readonly ValidationService _validationService;
    private readonly IMapper _mapper;

    public RpcValidationService(
        ILogger<RpcValidationService> logger,
        BotContext dbContext,
        ICacheClient cacheClient,
        ValidationService validationService,
        IMapper mapper)
    {
        _logger = logger;
        _dbContext = dbContext;
        _cacheClient = cacheClient;
        _validationService = validationService;
        _mapper = mapper;
    }

    public override async Task<RpcReceipt> ValidateReceipt(RpcValidateReceiptRequest rpcValidateReceiptRequest, ServerCallContext context)
    {
        RpcReceipt receipt = await _validationService.ValidateReceiptAsync(rpcValidateReceiptRequest.Base64String, context.CancellationToken);
        if (receipt == null)
            throw new InvalidOperationException("receipt is null");

        string cacheKey = $"receipt:{receipt.Code}";
        
        RpcReceipt? cached = await _cacheClient.GetAsync<RpcReceipt>(cacheKey, context.CancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation($"Receipt with code {receipt.Code} found in cache.");
            return cached;
        }
        
        Receipt dbReceipt = _mapper.Map<Receipt>(receipt);
        var existing = await _dbContext.Receipts.FirstOrDefaultAsync(r => r.Code == dbReceipt.Code, context.CancellationToken);

        if (existing is null)
        {
            await _dbContext.Receipts.AddAsync(dbReceipt, context.CancellationToken);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            receipt.Id = dbReceipt.Id;
        }
        else
        {
            receipt.Id = existing.Id;
        }

        await _cacheClient.SetAsync(cacheKey, receipt, TimeSpan.FromHours(6), context.CancellationToken);
        _logger.LogInformation($"Receipt with code {receipt.Code} saved to cache.");

        return receipt;
    }
}
