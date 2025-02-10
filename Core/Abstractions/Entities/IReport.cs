    using ValueObjects;

    namespace Abstractions;

    public interface IReport
    {
        public Guid CaseId { get; }
        
        public ConversionRatio ConversionRatio { get; }
        
        public PaymentsCount PaymentsCount { get; }
    }