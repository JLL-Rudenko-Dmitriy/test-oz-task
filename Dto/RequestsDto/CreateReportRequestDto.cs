namespace Dto.RequestsDto;

public class CreateReportRequestDto
{
    public DateTime DateStart { get; init; }
    
    public DateTime DateEnd { get; init; }
    
    public Guid CaseId { get; init; }
    
    public Guid ProductId { get; init; }
}