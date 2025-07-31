namespace SubscriptionService.Configs;

public class RabbitMqOptions
{
    public string Host { get; set; } = null!;
    
    public string Username { get; set; } = null!;
    
    public string Password { get; set; } = null!;
    
    public string Queue { get; set; } = null!;
}
