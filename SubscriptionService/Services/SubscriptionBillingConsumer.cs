using MassTransit;
using SubscriptionService.Interfaces;
using SubscriptionService.Models;

namespace SubscriptionService.Services;

public class SubscriptionBillingConsumer : IConsumer<SubscriptionBillingMessage>
{
    private readonly ISubscriptionService _logic;

    public SubscriptionBillingConsumer(ISubscriptionService logic)
    {
        _logic = logic;
    }

    public async Task Consume(ConsumeContext<SubscriptionBillingMessage> context)
    {
        await _logic.ProcessBillingAsync(context.Message);
    }
}