using Abstractions;
using Abstractions.Builders.ReportQueryBuilders;
using Enums;
using ValueObjects;

namespace Models.ReportQueries.Builders;

public class ReportQueryBuilder : IReportQueryBuilder
{
    private short? _status;
    private Period? _period;
    private Guid? _caseId;
    private Guid? _productId;

    public IReportQueryBuilder WithStatus(ProcessStatuses status)
    {
        _status = (short)status;
        return this;
    }

    public IReportQueryBuilder WithPeriod(Period period)
    {
        _period = period;
        return this;
    }

    public IReportQueryBuilder WithCaseId(Guid caseId)
    {
        _caseId = caseId;
        return this;
    }

    public IReportQueryBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public IReportQuery Build()
    {
        return new ReportQuery(
            _status ?? throw new ArgumentNullException(nameof(_status)),
            _period ?? throw new ArgumentNullException(nameof(_period)),
            _caseId ?? throw new ArgumentNullException(nameof(_caseId)),
            _productId ?? throw new ArgumentNullException(nameof(_productId)));
    }
}