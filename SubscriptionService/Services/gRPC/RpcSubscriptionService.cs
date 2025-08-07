using Grpc.Core;
using RpcSubscriptionService;
using SubscriptionService.Interfaces;
using static RpcSubscriptionService.RpcSubscriptionService;

namespace SubscriptionService.Services.gRPC;

public class RpcSubscriptionService: RpcSubscriptionServiceBase
{
    private readonly ISubscriptionService _logic;

    public RpcSubscriptionService(ISubscriptionService logic)
    {
        _logic = logic;
    }

    public override async Task<RpcSubscriptionStatusResponse> IsActive(RpcSubscriptionStatusRequest request, ServerCallContext context)
    {
        var isActive = await _logic.IsActiveAsync(request.UserId);
        return new RpcSubscriptionStatusResponse { IsActive = isActive };
    }

    public override async Task<RpcSubscriptionEndDateResponse> GetEndDate(RpcSubscriptionEndDateRequest request, ServerCallContext context)
    {
        var endDate = await _logic.GetEndDateAsync(request.UserId);
        return new RpcSubscriptionEndDateResponse
        {
            EndDate = endDate?.ToString("O") ?? string.Empty
        };
    }
}