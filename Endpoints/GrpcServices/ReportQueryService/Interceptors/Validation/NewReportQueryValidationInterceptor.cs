using Grpc.Core;
using Grpc.Core.Interceptors;
using ReportQueryManaging;
using System.Text.RegularExpressions;

namespace ReportQueryService.Interceptors.Validation;

public partial class NewReportQueryValidationInterceptor : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (request is GetReportRequest reportRequest)
        {
            if (!GuidRegex().IsMatch(reportRequest.CaseId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Case UUID is invalid"));
            }
        }
        
        return base.UnaryServerHandler(request, context, continuation);
    }
    
    [GeneratedRegex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
    private static partial Regex GuidRegex();
}

