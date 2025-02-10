using Grpc.Core;
using Grpc.Core.Interceptors;
using ReportQueryManaging;
using System.Text.RegularExpressions;

namespace ReportService.Interceptors.Validation;

public partial class GetReportValidationInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (request is ReportRequest reportRequest)
        {
            if (string.IsNullOrEmpty(reportRequest.DateStart) || string.IsNullOrEmpty(reportRequest.DateEnd))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Period: (start_date, end_date) are required"));
            }
            
            if (!TimeStampRegex().IsMatch(reportRequest.DateStart) ||
                !TimeStampRegex().IsMatch(reportRequest.DateEnd))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Incorrect date format"));
            }

            var startDate = DateTime.Parse(reportRequest.DateStart);
            var endDate = DateTime.Parse(reportRequest.DateEnd);

            if (startDate > endDate || endDate > DateTime.Today)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Date values are invalid"));
            }

            if (!GuidRegex().IsMatch(reportRequest.CaseId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Case UUID is invalid"));
            }
            
            if (!GuidRegex().IsMatch(reportRequest.ProductId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product UUID is invalid"));
            }
        }

        return await base.UnaryServerHandler<TRequest, TResponse>(request, context, continuation);
    }

    [GeneratedRegex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
    private static partial Regex GuidRegex();
    
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}(\.\d{1,6})?$")]
    private static partial Regex TimeStampRegex();
}