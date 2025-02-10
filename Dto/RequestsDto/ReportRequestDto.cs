namespace Dto.RequestsDto;

public record ReportRequestDto
{
    public DateTime DateStart { get; init; }
    
    public DateTime DateEnd { get; init; }
    
    public Guid ProductId { get; init; }

    public Guid CaseId { get; init; }
}