using Common.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Common.Interceptors;

public class ExceptionHandlingInterceptor: Interceptor
{
    private readonly ILogger<ExceptionHandlingInterceptor> _logger;

    public ExceptionHandlingInterceptor(ILogger<ExceptionHandlingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Бизнес-ошибка");

            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }        
        catch (InvalidReceiptException ex)
        {
            _logger.LogWarning(ex, "Неправильный чек");

            throw new InvalidReceiptException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка");

            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    } 
}