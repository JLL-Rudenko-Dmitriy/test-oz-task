namespace Dto.ResponsesDto;

public record ReportStatusResponseDto
{
    public bool IsSuccess { get; init; }
    
    public string? Status { get; init; } = null;
    
    public string? Message { get; init; } = null;
}