namespace Dto.ResponsesDto;

public record ReportResponseDto
{
    public decimal ConversionRatio { get; init; }
    
    public ulong PaymentsCount { get; init; }
    
    public bool IsSuccess { get; init; }
    
    public string ErrorMessage { get; init; } = string.Empty;
}